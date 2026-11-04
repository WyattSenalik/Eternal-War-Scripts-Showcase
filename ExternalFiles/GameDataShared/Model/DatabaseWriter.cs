// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Serialization;
using System.IO;

namespace EternalWar.Core.Model
{
    public static class DatabaseWriter
    {
        public static void WriteDatabase(string rootWriteToFolderPath, GameDatabase database)
        {
            WriteDataToFolder(rootWriteToFolderPath, DatabaseFolderNames.COMBATS_FOLDER_NAME, database.Combats);
            WriteDataToFolder(rootWriteToFolderPath, DatabaseFolderNames.UNITS_FOLDER_NAME, database.Units);
            WriteDataToFolder(rootWriteToFolderPath, DatabaseFolderNames.ACTION_SEQUENCES_FOLDER_NAME, database.ActionSequences);
            WriteDataToFolder(rootWriteToFolderPath, DatabaseFolderNames.PASSIVES_FOLDER_NAME, database.Passives);
            WriteDataToFolder(rootWriteToFolderPath, DatabaseFolderNames.NON_STACKABLE_EFFECTS_FOLDER_NAME, database.NonStackableEffects);
            WriteDataToFolder(rootWriteToFolderPath, DatabaseFolderNames.ARTIFACTS_FOLDER_NAME, database.Artifacts);
            WriteDataToFolder(rootWriteToFolderPath, DatabaseFolderNames.BLOCKS_FOLDER_NAME, database.Blocks);
        }

        public static void WriteDataToFolder<TValue>(string rootWriteToFolderPath, string parentFolderName, ValueDictionary<string, TValue> data) where TValue : IHaveStringId
        {
            string parentDataFolderPath = $"{rootWriteToFolderPath}/{parentFolderName}";
            if (!Directory.Exists(parentDataFolderPath))
                Directory.CreateDirectory(parentDataFolderPath);

            foreach (TValue value in data.Values)
            {
                string id = value.Id;
                string path = $"{rootWriteToFolderPath}/{parentFolderName}/{id}.json";
                string serialized = GameDataSerialization.Serialize(value);
                if (File.Exists(path))
                    File.Delete(path);

                using StreamWriter writer = File.CreateText(path);
                writer.Write(serialized);
                writer.Close();
            }
        }
    }
}
