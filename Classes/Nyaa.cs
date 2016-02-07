﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using HtmlAgilityPack;
using System;
using System.Net;

namespace anime_downloader.Classes {
    public class Nyaa {

        private static readonly Dictionary<string, double> toMegabyte = new Dictionary<string, double> {
            { "MiB", 1.04858  },
            { "GiB", 1073.74  },
            { "KiB", 0.001024 }
        };

        public string name;
        public string link;
        public string description;
        public string measurement;
        public int seeders;
        public double size;

        public Nyaa(HtmlNode node) {
            name = node.Element("title").InnerText;
            link = node.Element("#text").InnerText.Replace("#38;", "");
            description = node.Element("description").InnerText;
            if (description.Contains("CDATA"))
                description = description
                                        .Split(new string[] {"<![CDATA["}, StringSplitOptions.None)[1]
                                        .Split(new string[] {"]]>"}, StringSplitOptions.None)[0];
            seeders = int.Parse(description.Split(new string[] {" seeder"}, StringSplitOptions.None)[0]);
            measurement = toMegabyte.Where(d => description.Contains(d.Key)).First().Key;
            size =
                Math.Round(double.Parse(description.Split(new string[] {$" {measurement}"}, StringSplitOptions.None)[0]
                    .Split(new string[] {" - "}, StringSplitOptions.None)[1])
                           *toMegabyte[measurement],
                    2);
        }

        public override string ToString() => $"Nyaa<name={name}, link={link}, size={size} MB>";

        public string torrentName() {
            using (WebClient client = new WebClient()) {
                client.OpenRead(link);
                var disposition = client.ResponseHeaders["content-disposition"];
                var filename = disposition.Split(new string[] {"filename=\""}, StringSplitOptions.None)[1].Split('"')[0];
                return filename;
            }
        }

        public string strippedName(bool removeEpisode=false) {
            List<string> phrases = new List<string>();
            string text = name;

            foreach (Match match in Regex.Matches(text, @"\s *\[(.*?)\]|\((.*?)\)\s*"))
                phrases.Add(match.Groups[0].Value);
            foreach(String phrase in phrases)
                text = text.Replace(phrase, "");
            if (removeEpisode)
                text = String.Join("-", text.Split('-').Take(text.Split('-').Length-1).ToArray());
             
            return Regex.Replace(text.Trim(), @"\s+", " ");
            
        }

        public string subgroup() {
            foreach (Match match in Regex.Matches(name, @"\[([A-Za-z0-9_]+)\]+")) {
                var result = match.Groups[1].Value;
                if (result.All(c => !Char.IsNumber(c)))
                    return result;
            }
            return null;
        }

        public bool hasSubgroup() => subgroup() != null;

    }
}