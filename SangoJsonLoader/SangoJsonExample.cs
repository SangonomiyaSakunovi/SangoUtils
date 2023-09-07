using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static SangoJsonLoader.SangoJsonConverters;

//Developer: SangonomiyaSakunovi

namespace SangoJsonLoader
{
    public class SangoJsonExample
    {
        private ConcurrentDictionary<int, SangoJsonInfoExample> _sangoJsonInfoExampleDict = new ConcurrentDictionary<int, SangoJsonInfoExample>();

        private void LoadExample()
        {
            List<SangoJsonDataExample> sangoJsonDataExamples = JsonTools.LoadJsonFile<List<SangoJsonDataExample>>("Path");
            for (int index = 0; index < sangoJsonDataExamples.Count; index++)
            {
                SangoJsonDataExample data = sangoJsonDataExamples[index];
                SangoJsonInfoExample example = null;
                if (data.child != null)
                {
                    string[] intStrs = data.child.Split('|');
                    List<string> filteredStrs = new List<string>();
                    for (int i = 0; i < intStrs.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(intStrs[i]))
                        {
                            filteredStrs.Add(intStrs[i]);
                        }
                    }
                    int[] filteredInts = new int[filteredStrs.Count];
                    for (int j = 0; j < filteredStrs.Count; j++)
                    {
                        filteredInts[j] = int.Parse(filteredStrs[j]);
                    }
                    example = new SangoJsonInfoExample(data.id, data.resId, data.score, data.enumExample, filteredInts);
                    intStrs = null;
                    filteredStrs = null;
                    filteredInts = null;
                }
                else
                {
                    example = new SangoJsonInfoExample(data.id, data.resId, data.score, data.enumExample, null);
                }
                _sangoJsonInfoExampleDict.TryAdd(example.Id, example);
            }
            sangoJsonDataExamples = null;
        }
    }

    public class SangoJsonDataExample
    {

        [JsonConverter(typeof(NullableInt32Converter))]
        public int id { get; set; }
        [JsonConverter(typeof(NullableStringConverter))]
        public string resId { get; set; }
        [JsonConverter(typeof(NullableFloatConverter))]
        public float score { get; set; }
        [JsonConverter(typeof(NullableEnumConverter<SangoJsonEnumExample>))]
        public SangoJsonEnumExample enumExample { get; set; }
        public string child { get; set; }
    }

    public class SangoJsonInfoExample
    {
        public SangoJsonInfoExample(int id, string resId, float score, SangoJsonEnumExample enumExample, int[] childs)
        {
            Id = id;
            ResId = resId;
            Score = score;
            EnumExample = enumExample;
            Childs = childs;
        }

        public int Id { get; private set; }
        public string ResId { get; private set; }
        public float Score { get; private set; }
        public SangoJsonEnumExample EnumExample { get; private set; }
        public int[] Childs { get; private set; }

    }

    public enum SangoJsonEnumExample
    {
        None = 0,
        First = 1,
        Second = 2
    }
}
