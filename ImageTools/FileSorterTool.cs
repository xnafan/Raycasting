using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.IO;
namespace ImageTools
{
    /// <summary>
    /// Summary description for ThumbnailTool
    /// </summary>
    public static class FileSorterTool
    {

        public class FileNameSorter : IComparer<FileInfo>
        {
            #region IComparer<FileInfo> Members

            public int Compare(FileInfo x, FileInfo y)
            {
                return x.Name.CompareTo(y.Name);
            }

            #endregion
        }
        public class FileNamePriorityNameSorter : IComparer<FileInfo>
        {
            private string _priorityString;
            public string PriorityString
            {
                get { return _priorityString; }
                set { this._priorityString = value; }
            }

            public FileNamePriorityNameSorter(string prioritystring)
            {
                this.PriorityString = prioritystring;
            }

            #region IComparer<FileInfo> Members

            public int Compare(FileInfo x, FileInfo y)
            {
                bool xContainsString = x.Name.Contains(PriorityString);
                bool yContainsString = y.Name.Contains(PriorityString);
                if (xContainsString || yContainsString)
                {
                    //the NOT is important because the "prioritystring" should come first (e.g. lower than all others).
                    return -1 * xContainsString.CompareTo(yContainsString);
                }
                else
                {
                    return x.Name.CompareTo(y.Name);
                }
            }

            #endregion
        }
        public class FileSizeSorter : IComparer<FileInfo>
        {
            #region IComparer<FileInfo> Members

            public int Compare(FileInfo x, FileInfo y)
            {
                return x.Length.CompareTo(y.Length);
            }

            #endregion
        }
        public class FileCreationDateSorter : IComparer<FileInfo>
        {
            #region IComparer<FileInfo> Members

            public int Compare(FileInfo x, FileInfo y)
            {
                return x.CreationTime.CompareTo(y.CreationTime);
            }

            #endregion
        }
        public class FileChangedDateSorter : IComparer<FileInfo>
        {
            #region IComparer<FileInfo> Members

            public int Compare(FileInfo x, FileInfo y)
            {
                return x.LastWriteTime.CompareTo(y.LastWriteTime);
            }

            #endregion
        }
        public class FileExtensionSorter : IComparer<FileInfo>
        {
            #region IComparer<FileInfo> Members

            public int Compare(FileInfo x, FileInfo y)
            {
                return x.Extension.CompareTo(y.Extension);
            }

            #endregion
        }


        private static FileNameSorter _fileNameSorter = new FileNameSorter();
        private static FileSizeSorter _fileSizeSorter = new FileSizeSorter();
        private static FileCreationDateSorter _fileCreationDateSorter = new FileCreationDateSorter();
        private static FileChangedDateSorter _fileChangedDateSorter = new FileChangedDateSorter();
        private static FileExtensionSorter _fileExtensionSorter = new FileExtensionSorter();


        public static void SortByFileName(List<FileInfo> fileInfosToSort)
        {
            fileInfosToSort.Sort(_fileNameSorter);
        }
        public static void SortByFileNameWithPriority(List<FileInfo> fileInfosToSort, string priorityString)
        {
            fileInfosToSort.Sort(new FileNamePriorityNameSorter(priorityString));
        }
        public static void SortByFileSize(List<FileInfo> fileInfosToSort)
        {
            fileInfosToSort.Sort(_fileSizeSorter);
        }
        public static void SortByFileCreationDate(List<FileInfo> fileInfosToSort)
        {
            fileInfosToSort.Sort(_fileCreationDateSorter);
        }
        public static void SortByFileChangedDate(List<FileInfo> fileInfosToSort)
        {
            fileInfosToSort.Sort(_fileChangedDateSorter);
        }
        public static void SortByFileExtension(List<FileInfo> fileInfosToSort)
        {
            fileInfosToSort.Sort(_fileExtensionSorter);
        }

    }

}