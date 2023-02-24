using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiscordLeagueBot;

public class InsultGenerator
{
    private string _adjectivesFilePath = @"WordListFiles/adjectives.txt";
    private string _nounsFilePath = @"WordListFiles/nouns.txt";
    private string _insultSentencesWithNameFilePath = @"WordListFiles/InsultSentenceStructuresWithName.txt";
    
    private List<string> _adjectives = new();
    private List<string> _nouns = new();
    private List<string> _insultSentencesWithName = new();

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
        var sentence = _insultSentencesWithName[_rand.Next(0, _insultSentencesWithName.Count)];
        var adjective = _adjectives[_rand.Next(0, _adjectives.Count)];
        var noun = _nouns[_rand.Next(0, _nouns.Count)];

        sentence = sentence.Replace("[name]", name);
        sentence = sentence.Replace("[adj]", adjective);
        sentence = sentence.Replace("[noun]", noun);
        
        return sentence;
    }

    /// <summary>
    /// Update all word list files at once.
    /// </summary>
    public async Task UpdateWordListFiles()
    {
        await UpdateAdjectives();
        await UpdateNouns();
        await UpdateNamedSentenceStructures();
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

    /// <summary>
    /// Update sentence structures that use a name at the front.
    /// </summary>
    /// <exception cref="Exception">Throws an error if the file is not found.</exception>
    public async Task UpdateNamedSentenceStructures()
    {
        if (File.Exists(_insultSentencesWithNameFilePath))
        {
            var text = await File.ReadAllLinesAsync(_insultSentencesWithNameFilePath);
            _insultSentencesWithName = text.ToList();
            Console.WriteLine("Updated insult sentences with name.");
        }
        else
        {
            Console.WriteLine($"File not found at: {_insultSentencesWithNameFilePath}");
            throw new Exception($"File not found at: {_insultSentencesWithNameFilePath}");
        }
    }
}