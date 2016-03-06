﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace anime_downloader.Classes {
    public class Nyaa {
        /// <summary>
        ///     A conversion chart from any of these values to megabytes.
        /// </summary>
        private static readonly Dictionary<string, double> ToMegabyte = new Dictionary<string, double> {
            {"MiB", 1.04858},
            {"GiB", 1073.74},
            {"KiB", 0.001024}
        };

        /// <summary>
        ///     The description containing seeder & measurement information.
        /// </summary>
        public string Description;

        /// <summary>
        ///     The given download link.
        /// </summary>
        public string Link;

        /// <summary>
        ///     The unit of measurement used in size.
        /// </summary>
        public string Measurement;

        /// <summary>
        ///     The nyaa's parsed filename.
        /// </summary>
        public string Name;

        /// <summary>
        ///     The amount of people seeding the torrent.
        /// </summary>
        public int Seeders;

        /// <summary>
        ///     The size of the download.
        /// </summary>
        public double Size;

        /// <summary>
        ///     HTML Nyaa Initializer
        /// </summary>
        /// <param name="node">A raw node.</param>
        public Nyaa(HtmlNode node) {
            Name = WebUtility.HtmlDecode(node.Element("title").InnerText.Replace("Â", ""));
            Link = node.Element("#text").InnerText.Replace("#38;", "");
            Description = node.Element("description").InnerText;
            if (Description.Contains("CDATA"))
                Description = Description
                    .Split(new[] {"<![CDATA["}, StringSplitOptions.None)[1]
                    .Split(new[] {"]]>"}, StringSplitOptions.None)[0];
            Seeders = int.Parse(Description.Split(new[] {" seeder"}, StringSplitOptions.None)[0]);
            Measurement = ToMegabyte.Where(d => Description.Contains(d.Key)).First().Key;
            Size =
                Math.Round(double.Parse(Description.Split(new[] {$" {Measurement}"}, StringSplitOptions.None)[0]
                    .Split(new[] {" - "}, StringSplitOptions.None)[1])
                           *ToMegabyte[Measurement],
                    2);
        }

        /// <summary>
        ///     A simple representation of the important attribes of a Nyaa object.
        /// </summary>
        /// <returns>summary of nyaa values</returns>
        public override string ToString() => $"Nyaa<name={Name}, link={Link}, size={Size} MB>";

        /// <summary>
        ///     Gathers the torrent's filename from it's meta-data.
        /// </summary>
        /// <returns>A valid filename for the torrent.</returns>
        public string TorrentName() {
            var request = WebRequest.Create(Link) as HttpWebRequest;
            HttpWebResponse response = null;

            try {
                response = request?.GetResponse() as HttpWebResponse;
                response?.GetResponseStream();
                var disposition = response?.Headers["content-disposition"];
                var filename =
                    disposition?.Split(new[] {"filename=\""}, StringSplitOptions.None)[1].Split('"')[0];
                return filename;
            }

            catch {}

            finally {
                response?.Close();
            }

        
            return null;
        }

        /// <summary>
        ///     Strips the filename to remove extraneous information and returns name.
        /// </summary>
        /// <param name="removeEpisode">A flag for also removing the episode number</param>
        /// <returns>The name of the anime.</returns>
        public string StrippedName(bool removeEpisode = false) {
            var phrases = new List<string>();
            var text = Name;
            
            foreach (Match match in Regex.Matches(text, @"\s?\[(.*?)\]|\((.*?)\)\s*"))
                phrases.Add(match.Groups[0].Value);

            phrases.ForEach(p => text = text.Replace(p, ""));

            if (removeEpisode)
                text = string.Join("-", text.Split('-').Take(text.Split('-').Length - 1).ToArray());

            return Regex.Replace(text.Trim(), @"\s+", " ");
        }

        /// <summary>
        ///     Returns the subgroup from the name of the file.
        /// </summary>
        /// <returns>The subgroup of the file.</returns>
        public string Subgroup() {
            foreach (Match match in Regex.Matches(Name, @"\[([A-Za-z0-9_µ\-]+)\]+")) {
                var result = match.Groups[1].Value;
                if (result.All(c => !char.IsNumber(c)))
                    return result;
            }
            return null;
        }

        /// <summary>
        ///     A check if the subgroup exists.
        /// </summary>
        /// <returns>If a subgroup exists.</returns>
        public bool HasSubgroup() => Subgroup() != null;
    }
}