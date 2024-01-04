using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Function.Domain.Models.OL;
using Function.Domain.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Function.Domain.Helpers
{
    public class OlSynapseMessageConsolidation : IOlMessageConsolidation
    {
        private ILogger _log;
        private readonly IBlobClientFactory _blobClientFactory;
        private const string CONTAINER_NAME = "ol-synapsemessages";
        public OlSynapseMessageConsolidation(ILoggerFactory loggerFactory, IBlobClientFactory blobClientFactory)
        {
            _log = loggerFactory.CreateLogger<OlSynapseMessageConsolidation>();
            _blobClientFactory = blobClientFactory;
        }

        public async Task<Event?> ConsolidateEventAsync(Event olEvent, string runId)
        {
            try
            {
                var result = await CaptureMessageAsync(olEvent, runId);
                return await ConsolidateEventsAsync(olEvent, result);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"OlSynapseMessageConsolidation-ConsolidateEventAsync: Error {ex.InnerException} ", ex.Message);
                throw;
            }
        }

        private async Task<(List<string> Inputs, List<string> Outputs)> CaptureMessageAsync(Event olEvent, string runId)
        {
            string prefixInput = runId + "/Input/";
            string prefixOutput = runId + "/Output/";

            List<Task> inputUploadTasks = new List<Task>();
            List<Task> outputUploadTasks = new List<Task>();

            //Assumption: We assume the number of input / outputs is small so parallelism is currently unbounded.
            //TODO : Implement bounded parallel calls using batch/ chunk if there are huge input/outputs

            // Parallelize input uploads
            inputUploadTasks.AddRange(olEvent.Inputs.Select(async (item) =>
            {
                var inputJson = JsonConvert.SerializeObject(item);
                string inputBlobName = prefixInput + GetUniqueHash(item.Name, item.NameSpace);
                // Check if the blob already exists
                if (!await _blobClientFactory.ExistsAsync(CONTAINER_NAME, inputBlobName))
                {
                    return _blobClientFactory.UploadAsync(CONTAINER_NAME, inputBlobName, inputJson);
                }
                // If it already exists, return a completed task
                return Task.CompletedTask;
            }));

            // Parallelize output uploads
            outputUploadTasks.AddRange(olEvent.Outputs.Select(async (item) =>
            {
                var outputJson = JsonConvert.SerializeObject(item);
                string outputBlobName = prefixOutput + GetUniqueHash(item.Name, item.NameSpace);
                // Check if the blob already exists
                if (!await _blobClientFactory.ExistsAsync(CONTAINER_NAME, outputBlobName))
                {
                    return _blobClientFactory.UploadAsync(CONTAINER_NAME, outputBlobName, outputJson);
                }

                // If it already exists, return a completed task
                return Task.CompletedTask;
            }));

            // Await all upload tasks
            await Task.WhenAll(inputUploadTasks.Concat(outputUploadTasks));


            // Get list of input and outputs which are stored in blob
            List<string> inputs = await _blobClientFactory.GetBlobsByHierarchyAsync(prefixInput);
            List<string> outputs = await _blobClientFactory.GetBlobsByHierarchyAsync(prefixOutput);
            return (inputs, outputs);
        }

        private async Task<Event?> ConsolidateEventsAsync(Event olEvent, (List<string> Inputs, List<string> Outputs) result)
        {
            // If there is at least 1 input and 1 output - proceed for message consolidation
            if (result.Inputs != null && result.Inputs.Count > 0 && result.Outputs != null && result.Outputs.Count > 0)
            {
                List<Inputs?> inputsList = new List<Inputs?>();
                List<Outputs?> outputsList = new List<Outputs?>();

                // Get all the inputs
                List<Task<Inputs?>> inputTasks = result.Inputs.Select(async item =>
                {
                    var inputObject = await _blobClientFactory.DownloadBlobAsync(CONTAINER_NAME, item);
                    return JsonConvert.DeserializeObject<Inputs?>(inputObject);
                }).ToList();

                // Get all the outputs
                List<Task<Outputs?>> outputTasks = result.Outputs.Select(async item =>
                {
                    var outputObject = await _blobClientFactory.DownloadBlobAsync(CONTAINER_NAME, item);
                    return JsonConvert.DeserializeObject<Outputs?>(outputObject);
                }).ToList();

                inputsList = (await Task.WhenAll(inputTasks)).Where(input => input != null).ToList();
                outputsList = (await Task.WhenAll(outputTasks)).Where(output => output != null).ToList();

                // Message Consolidation - Update inputs / outputs to the current olevent object
                if (inputsList.Count > 0)
                {
                    olEvent.Inputs.Clear();
                    olEvent.Inputs.AddRange(inputsList);
                }

                // to do change
                if (outputsList.Count > 0)
                {
                    olEvent.Outputs.Clear();
                    olEvent.Outputs.AddRange(outputsList);
                }
                return olEvent;
            }
            else
            {
                return null;
            }
        }

        private string GetUniqueHash(string name, string namespaceValue)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string combinedValues = $"{name}{namespaceValue}";
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedValues));
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
        }
    }
}