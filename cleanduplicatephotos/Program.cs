using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cleanduplicatephotos
{
    class Program
    {
        /// <summary>
        /// Extensions file to look up
        /// </summary>
        static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", "NEF", "MOV" };

        /// <summary>
        /// Root folder to look up
        /// </summary>
        static readonly string rootFolder = $"/Volumes/FOTOS/";

        static void Main(string[] args)
        {
            var duplicates = GetDuplicates(rootFolder);
            foreach (var file in duplicates)
            {
                Console.WriteLine(file.Key);
                foreach (var data in file.Value)
                {
                    Console.WriteLine(data);
                }
            }
            Console.WriteLine($"Encontrados: {duplicates.Count()}");
            Console.WriteLine("Quieres borrar los archivos duplicados?(y/n)");
            var response = Console.ReadKey();
            if(response.KeyChar == 'y')
            {
                DeleteFiles(duplicates);
            }
        }

        static void DeleteFiles(Dictionary<string, List<FileDuplicated>> duplicates)
        {
            foreach (var duplicate in duplicates)
            {
                Console.WriteLine($"{duplicate.Key}: {duplicate.Value.Count()}");
                //except the last one
                foreach (var file in duplicate.Value.GetRange(0, duplicate.Value.Count() - 1))
                {
                    try
                    {
                        File.Delete(file.Path);
                        Console.WriteLine($"Archivo elimnado correctamente: {file.Path}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"No se pudo eliminar: {file.Path}: {ex.Message}");
                    }
                }
            }
        }

        static Dictionary<string, List<FileDuplicated>> GetDuplicates(string rootFolder)
        {
            var allFiles = GetAllFiles(rootFolder);
            //var dictionary = allFiles.ToDictionary(x => x.Split("/").Last());
            var gruopedFiles = allFiles.GroupBy(x => x.Split("/").Last()).Where(g => g.Count() > 1);
            var dictionary = new Dictionary<string, List<FileDuplicated>>();
            foreach (var file in gruopedFiles)
            {
                var listDuplucates = new List<FileDuplicated>();
                foreach (var fileDupliucated in file)
                {
                    var duplicated = new FileDuplicated
                    {
                        Path = fileDupliucated,
                        //LastModifiedDate = File.GetLastWriteTime(fileDupliucated),
                        //Attributes = File.GetAttributes(fileDupliucated),
                        Info = new FileInfo(fileDupliucated)
                    };

                    if(listDuplucates.Any())
                    {
                        //compare size
                        if(listDuplucates.First().Info.Length == duplicated.Info.Length)
                        {
                            listDuplucates.Add(duplicated);
                        }
                    } else
                    {
                        listDuplucates.Add(duplicated);
                    }
                }

                dictionary.Add(file.Key, listDuplucates.OrderBy(x => x.Info.LastWriteTime).ToList());
            }

            return dictionary;
        }

        static List<string> GetAllFiles(string path)
        {
            var directories = Directory.EnumerateDirectories(path);
            var subFiles = new List<string>();
            foreach (var dir in directories)
            {
                if (!dir.Contains("/."))
                {
                    subFiles.AddRange(GetAllFiles(dir));
                }
            }
            var files = Directory.EnumerateFiles(path);
            var filesList = files.ToList().Where(f => ImageExtensions.ToArray().Contains(f.Split('.').Last())).ToList();
            filesList.AddRange(subFiles);
            return filesList;
        }
    }

    class FileDuplicated
    {
        public string Path { get; set; }

        // public DateTime LastModifiedDate { get; set; }

        // public FileAttributes Attributes { get; set; }

        public FileInfo Info { get; set; }

        public override string ToString()
        {
            return $"{Path} - {Info.LastWriteTime}";
        }
    }
}
