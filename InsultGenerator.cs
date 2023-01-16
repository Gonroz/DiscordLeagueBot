using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiscordLeagueBot;

public class InsultGenerator
{
    private string _adjectivesFilePath = @"WordListFiles\adjectives.txt";
    private string _nounsFilePath = @"WordListFiles\nouns.txt";
    
    List<string> _adjectives = new();
    private List<string> _nouns = new();

    private Random _rand = new();

    /// <summary>
    /// An object used to generate random insults.
    /// </summary>
    public InsultGenerator()
    {
        Console.WriteLine("Insult Generator created");
    }

    /// <summary>
    /// Generates a generic random insult.
    /// </summary>
    /// <returns>A generic random insult in the form of a string.</returns>
    public async Task<string> GenerateRandomInsult()
    {
        return $"You are a {_adjectives[_rand.Next(0, _adjectives.Count)]} {_nouns[_rand.Next(0, _nouns.Count)]}.";
    }

    /// <summary>
    /// Generates a random insult against the person with 'name'.
    /// </summary>
    /// <param name="name">The name of the person to insult.</param>
    /// <returns>A randomly generated insult.</returns>
    public async Task<string> GenerateRandomInsult(string name)
    {
        return $"{name} is a {_adjectives[_rand.Next(0, _adjectives.Count)]} {_nouns[_rand.Next(0, _nouns.Count)]}.";
    }

    /// <summary>
    /// Update the list of strings in the object from the default file location.
    /// </summary>
    /// <exception cref="Exception">File not found exception.</exception>
    public async Task UpdateAdjectives()//string filePath)
    {
        //var filePath = adjectivesFilePath;
        //filePath = @"WordListFiles\adjectives.txt";

        if (File.Exists(_adjectivesFilePath))
        {
            var text = await File.ReadAllLinesAsync(_adjectivesFilePath);
            _adjectives = text.ToList();
            Console.WriteLine("Updated list of adjectives.");
        }
        else
        {
            Console.WriteLine($"File not found: {_adjectivesFilePath}");
            throw new Exception($"File not found at: {_adjectivesFilePath}");
        }
    }

    /// <summary>
    /// Update the list of nouns in the object from the default file location.
    /// </summary>
    /// <exception cref="Exception">File not found exception.</exception>
    public async Task UpdateNouns()
    {
        if (File.Exists(_nounsFilePath))
        {
            var text = await File.ReadAllLinesAsync(_nounsFilePath);
            _nouns = text.ToList();
            Console.WriteLine("Updated list of nouns.");
        }
        else
        {
            Console.WriteLine($"File not found: {_nounsFilePath}");
            throw new Exception($"File not found at: {_nounsFilePath}");
        }
    }
}