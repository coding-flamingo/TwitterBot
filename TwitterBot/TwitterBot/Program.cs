// See https://aka.ms/new-console-template for more information
using TwitterAPIWrapper.Models;
using TwitterAPIWrapper.Services;

Console.WriteLine("Follower Finder");

ISecretService _secretService = new AKVSecretService(
    "https://codingflamingotest2.vault.azure.net/");
string secret = await 
    _secretService.GetKeyVaultSecretAsync("twitterToken");
ITwitterService _twitterService = new TwitterService(new(), secret);
string? userID = await _twitterService.GetUserIDAsync("coding_flamingo");
if(userID == null)
{
    Console.WriteLine("Error getting user ID from Twitter");
    return;
}
FollowersResponseModel? userFollowers = await 
    _twitterService.GetUserFollowersAsync(userID);
FollowersResponseModel? userFollowing = await 
    _twitterService.GetUserIsFollowingAsync(userID);
if(userFollowers != null 
    && userFollowing != null)
{
    List<FollowerDetails> followerYouDontFollow =
        userFollowers.Data.Where(i =>
        userFollowing.Data.Select(x => 
            x.Id).Contains(i.Id) == false).ToList();
    List<FollowerDetails> peopleDontFollowBack =
        userFollowing.Data.Where(i =>
        userFollowers.Data.Select(x => 
            x.Id).Contains(i.Id) == false).ToList();
    Console.WriteLine($"" +
        $"{followerYouDontFollow.Count} " +
        $"followers follow " +
        $"you and you don't follow back:");
    foreach(FollowerDetails follower in followerYouDontFollow)
    {
        Console.WriteLine($"UserName: " +
            $"{follower.Username} " +
            $"Name:{follower.Name}");
    }
    Console.WriteLine(
        $"{peopleDontFollowBack.Count} don't" +
        $" follow you back:");
    foreach (FollowerDetails follower in 
        peopleDontFollowBack)
    {
        Console.WriteLine($"UserName: " +
            $"{follower.Username} " +
            $"Name:{follower.Name}");
    }
}
else
{
    Console.WriteLine("There was an error " +
        "getting information from Twitter");
}