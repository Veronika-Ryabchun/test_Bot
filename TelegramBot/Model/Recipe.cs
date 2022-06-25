using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Model
{
    public class Results
    {
        public List<ResultItem> results { get; set; }
    }
    public class ResultItem
    {
        public string name { get; set; }
        public string description { get; set; }
        public string original_video_url { get; set; }
        public List<InstructionItem> instructions { get; set; }
        public List<TagItem> tags { get; set; }
    }
    public class TagItem
    {
        public string display_name { get; set; }
        public string type { get; set; }
        public string name { get; set; }
    }
    public class InstructionItem
    {
        public int position { get; set; }
        public string display_text { get; set; }
    }
}
