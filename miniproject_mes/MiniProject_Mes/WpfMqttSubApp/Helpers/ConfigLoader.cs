﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfMqttSubApp.Models;

namespace WpfMqttSubApp.Helpers
{
    public static class ConfigLoader
    {
        public static TotalConfig Load(string path = "./config.json")
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("설정파일이 없습니다.", path);
            }

            string json = File.ReadAllText(path);
            var config = JsonConvert.DeserializeObject<TotalConfig>(json);

            if (config == null)
            {
                throw new InvalidDataException("설정파일을 읽을 수 없습니다.");
            }
            return config;
        }
    }
}
