using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using TwitterAPIWrapper.Models;

namespace TwitterAPIWrapper.Services;

public interface ITwitterService
{
    Task<FollowersResponseModel?> GetUserFollowersAsync(string userID);
    Task<FollowersResponseModel?> GetUserIsFollowingAsync(string userID);
    Task<string?> GetUserIDAsync(string userName);
}

public class TwitterService : ITwitterService
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly string _accessToken;
    public TwitterService(HttpClient httpClient, string token)
    {
        _httpClient = httpClient;
        HttpStatusCode[] httpStatusCodesWorthRetrying = {
           HttpStatusCode.RequestTimeout, // 408
           HttpStatusCode.InternalServerError, // 500
           HttpStatusCode.BadGateway, // 502
           HttpStatusCode.ServiceUnavailable, // 503
           HttpStatusCode.GatewayTimeout // 504
        };
        _accessToken = token;
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrInner<TaskCanceledException>()
            .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
              .WaitAndRetryAsync(new[]
              {
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8)
              });
    }

    public async Task<FollowersResponseModel?> GetUserFollowersAsync(string userID)
    {
        if(string.IsNullOrEmpty(userID))
        {
            throw new ArgumentNullException(nameof(userID));
        }
        string url = $"https://api.twitter.com/2/users/{userID}/followers";
        APIResultModel apiResult = await CallGeneric(url, string.Empty, HttpMethod.Get);
        if(apiResult.Success)
        {
            try
            {
                return JsonSerializer.Deserialize<FollowersResponseModel>(apiResult.Message);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error reading followers response: " + ex.Message);
            }
        }
        return null;
    }

    public async Task<FollowersResponseModel?> GetUserIsFollowingAsync(string userID)
    {
        if (string.IsNullOrEmpty(userID))
        {
            throw new ArgumentNullException(nameof(userID));
        }
        string url = $"https://api.twitter.com/2/users/{userID}/following";
        APIResultModel apiResult = await CallGeneric(url, string.Empty, HttpMethod.Get);
        if (apiResult.Success)
        {
            try
            {
                return JsonSerializer.Deserialize<FollowersResponseModel>(apiResult.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading followers response: " + ex.Message);
            }
        }
        return null;
    }

    public async Task<string?> GetUserIDAsync(string userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            throw new ArgumentNullException(nameof(userName));
        }
        string url = $"https://api.twitter.com/2/tweets/search/recent?query=from:{userName}&tweet.fields=created_at&expansions=author_id&user.fields=created_at";
        APIResultModel apiResult = await CallGeneric(url, string.Empty, HttpMethod.Get);
        if (apiResult.Success)
        {
            try
            {
                UserTweetsModel? response =  JsonSerializer.Deserialize<UserTweetsModel>(apiResult.Message);
                if(response != null)
                {
                    return response.Includes.Users.FirstOrDefault()?.ID;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading followers response: " + ex.Message);
            }
        }
        return null;
    }

    private async Task<APIResultModel> CallGeneric(string url, string jsonPayload, HttpMethod method)
    {
        APIResultModel apiResult = new APIResultModel();
        HttpResponseMessage responseMessage;
        HttpRequestMessage requestMessage = new HttpRequestMessage(method, url);
        _httpClient.DefaultRequestHeaders.Clear();
        try
        {
            if (!string.IsNullOrWhiteSpace(jsonPayload))
            {
                requestMessage.Content = new StringContent(jsonPayload,
                    Encoding.UTF8, "application/json");
            }
            if (!string.IsNullOrWhiteSpace(_accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);
            }
            responseMessage = await _retryPolicy.ExecuteAsync(async () =>
                      await SendMessageAsync(requestMessage));
            apiResult.Message = await responseMessage.Content.ReadAsStringAsync();
            apiResult.Success = responseMessage.IsSuccessStatusCode;
            if (!apiResult.Success)
            {
                Console.WriteLine($"Error calling API {responseMessage.StatusCode} " +
                    $"{apiResult.Message}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling API {ex.Message}");
            apiResult.Success = false;
            apiResult.Message = "Error contacting server, please try again later";
        }
        return apiResult;
    }


    private async Task<HttpResponseMessage> SendMessageAsync(HttpRequestMessage requestMessage)
    {
        HttpResponseMessage response;
        response = await _httpClient.SendAsync(requestMessage);
        return response;
    }
}