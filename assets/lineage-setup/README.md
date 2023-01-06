## Setup utilities for configuring lineage for Azure Synapse Analytics Spark Pool and Microsoft Purview

The utilities in this repo helps to simplify resources required for configuring Azure Synapse Spark pools to emit lineage information into Microsoft Purivew. 

## Prerequisites

This utility provisions resources in a Virtual Network with Private Endpoints. Hence the utlities must be run from a CLI enviroment which as access to the private endpoints. Typically from an on-premises VM which has network connectivity to Azure Virtual network.

* Linux Bash CLI. The utility leverages several linux packages such as jq and bash scripting which needs a Linux bash CLI. The utility can run in any of the below environments. 

    macOS Terminal 
    Windows WSL 
    Linux 
    Azure Cloud Shell - Bash ( this already has az cli installed)

* Install az cli with one command

    ```bash
    curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
    ```

* Login into Azure using az cli. The credentials should have permission to provision Azure Resources.

    ```bash
    az login
    ```
    

* Azure Active Directory Service Principals and Secret

* Network connectivity to Azure Virtual Network

## How to run utility

1. Clone repo

    ```bash
    git clone https://github.com/anildwarepo/Purview-ADB-Lineage-Solution-Accelerator.git
    ```

2. Provision Azure Resources that creates the required components to capture and process spark lineage. Follow prompts for needed input parameters.

    ```bash
    ./setup-lineage-cloudshell.sh
    ```
    **Note** This utility can be run from Azure Cloud Shell without requiring network connectivity to Azure Virtual Network.

3. Assign Synapse Spark role definitions

    ```bash
    ./assign-synapse-roles-run-within-network.sh.sh
    ```
    **Note** This utility requires network connectivity to Azure Virtual Network.
    
4. Create purivew typedefs and entities

    ```bash
    ./create-purview-typedefs-relationship-run-within-network.sh
    ```
    **Note** This utility requires network connectivity to Azure Virtual Network.