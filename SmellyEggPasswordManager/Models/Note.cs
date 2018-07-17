using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmellyEggPasswordManager.Models
{
    /// <summary>
    /// 日报类
    /// </summary>
    public class Note
    {
        private string noteType = string.Empty;

        private string noteContent = string.Empty;

        private string noteText = string.Empty;

        private string userName = string.Empty;

        private string noteTitle = string.Empty;

        public string NoteType { get => noteType; set => noteType = value; }

        public string NoteContent { get => noteContent; set => noteContent = value; }

        public string UserName { get => userName; set => userName = value; }

        public string NoteTitle { get => noteTitle; set => noteTitle = value; }
        public string NoteText { get => noteText; set => noteText = value; }
    }
}
