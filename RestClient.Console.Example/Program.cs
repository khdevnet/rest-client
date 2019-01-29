using System;

namespace RestClientConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {

            var githubClient = new GitHubClient();

            var emojis = githubClient.GetEmojis();


            Console.WriteLine(emojis.Data["1st_place_medal"]);
            Console.ReadLine();
        }
    }
}
