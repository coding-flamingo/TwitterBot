using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TwitterAPIWrapper.Models;

namespace TwitterAPIWrapper.Services;
public interface ISecretService
{
    Task<string> GetKeyVaultSecretAsync(string secretName);
    Task<X509Certificate2> GetKeyVaultCertAsync(string secretName);
    Task<APIResultModel> CreateSecretAsync(string secretName, string secretContent);
    Task<APIResultModel> DeleteSecretAsync(string secretName);
}

public class AKVSecretService : ISecretService
{
    private readonly SecretClient _akvSecretClient;
    public AKVSecretService(  string keyVault)
    {
        _akvSecretClient = new SecretClient(new Uri(keyVault), new DefaultAzureCredential());
    }

    public async Task<string> GetKeyVaultSecretAsync(string secretName)
    {
        if (secretName.Contains("/secrets/"))
        {
            secretName = secretName.Split("/secrets/")[1].Split("/")[0];
        }
        string secret;
        KeyVaultSecret returnedSecret = await _akvSecretClient.GetSecretAsync(secretName);
        secret = returnedSecret.Value;
        return secret;
    }

    public async Task<X509Certificate2> GetKeyVaultCertAsync(string secretName)
    {
        string secret = await GetKeyVaultSecretAsync(secretName);
        X509Certificate2 cert = new(Convert.FromBase64String(secret));
        return cert;
    }

    public async Task<APIResultModel> CreateSecretAsync(string secretName, string secretContent)
    {
        try
        {
            var akvResponse = await _akvSecretClient.SetSecretAsync(secretName, secretContent);
            return new APIResultModel(true, akvResponse.Value.Id.AbsoluteUri);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error Saving Secret in AKV " + ex.Message);
        }
        return new APIResultModel(false, "Error saving request, please try again later.");
    }

    public async Task<APIResultModel> DeleteSecretAsync(string secretName)
    {
        try
        {
            var response = await _akvSecretClient.StartDeleteSecretAsync(secretName);
            return new APIResultModel(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine( "Error Deleting Secret in AKV " + ex.Message);
            return new APIResultModel(false, "Error saving request, please try again later.");
        }
    }
}