This is an example of an authorization proxy for an Azure Storage Account. It provides authorized access to generic files in a storage account using the Valet Key pattern - e.g. if you have a storage account with blobs in it, and need to provide discretionary access to those files to authenticated people.

Then this pattern consists in putting a proxy in front of the storage account, have this proxy handle authorization, and upon successful auth/authZ, redirect the client to the blob with a short-lived Shared Access Signature.

So if the file is stored here: `https://demo123.blob.core.windows.net/files/happy.jpg`, then the URL provided to the users will be `https://functionapp123.azurewebsites.net/files/happy.jpg`. When directed there, the function will authorize the call then generate a SAS and redirect to the url (e.g. `https://demo123.blob.core.windows.net/files/happy.jpg?sv=2018-03-28&sr=b&sig=VEpGsNghVV8QEyq18z%2F8R6rW7RuY8kta1sAdHMnq8PM%3D&st=2019-04-12T23%3A13%3A08Z&se=2019-04-12T23%3A33%3A08Z&sp=r`)

# Alternative

An alternative is to use [AAD-backed RBAC for blobs](https://docs.microsoft.com/en-us/azure/storage/common/storage-auth-aad?toc=%2fazure%2fstorage%2fblobs%2ftoc.json#rbac-roles-for-blobs-and-queues). This is an easier solution but it has limitations:

- Permissions are set at a container level. If you need finer grained authorization, then a proxy will give you that flexibility
- If you are not using AAD, then this pattern allows you to handle authorization the way you need to.

# To deploy the demo

This demo contains a provisioning script that will automatically provision the necessary
resources and deploy the application. You need an Azure Subscription for that.

The provisioning script will create:
- A resource group
- A storage account
- A pay as you go function app (that will remain free under a few million calls...)
- An app registration - you need access relevant access rights to your AAD tenant to deploy it.

For it to work you have two options:

1. **If you have the Azure CLI installed and setup on a Linux terminal on your machine (either Linux,
    a VM, or the WSL)**. then make sure you are currently
    selecting the subscription on which you want the demo to be deployed, clone the Git repo,
    then simply run `./provision.sh`.

2. **If you don't have the Azure CLI installed**, go to the [Azure Shell](https://shell.azure.com),
    select the subscription on which you want to provision the demo, then select "Bash" as
    the shell type, and finally, run 
    `git clone https://github.com/cfe84/demo-storage-auth-proxy.git && cd demo-storage-auth-proxy && chmod +x provision.sh && ./provision.sh`.

Once the demo is deployed, you can upload files to the storage account container called "files", and those will be available by point to `https://[YOUR FUNCTION APP URL]/files/filename.txt`.

# To cleanup the demo

Overall the resources created for that demo are free or practically free. However there's 
no security setup so you should delete the resources after you are done.

The provisioning script creates a cleanup script to delete all the resources it created.

Run `./cleanup.sh` on the same place you ran the initial command (local CLI, or Azure Shell).

Alternatively, simply delete the resource group that was created.

# To run the demo locally

This requires dotnet core 2.2 on the machine you are running.

You need to provision a storage account and a container within it ; and then update the `local.settings.json` to point to your storage account.

Go to the `src` folder and run `dotnet restore`, then `dotnet run`. The application should start
on port 5000, so navigate to `http://localhost:5000` ; potentially ignore HTTPS warnings.