using System.ComponentModel.DataAnnotations;

namespace LibraryLocationQuerySystem.Utilities
{
    /// <summary>
    /// 分页管理
    /// </summary>
    public class PageManager
    {
        public int NumPerPage { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int ResNum { get; set; }
        public int CurrentPage { get; set; }
        public int PreviousPage { get; set; }
        public int NextPage { get; set; }
        public int JumpPage { get; set; }
        public void Set(int pageNum, int resNum)
        {
			if (pageNum < 0) pageNum = 0;
			StartIndex = pageNum * NumPerPage;
            ResNum = resNum;
			                    //即将越界
			NextPage = (NextPage < ResNum / NumPerPage) ? pageNum + 1 : ResNum / NumPerPage;
		    //越界
			if (ResNum <= StartIndex)
            {
                pageNum = ResNum / NumPerPage;
                StartIndex = pageNum * NumPerPage;
                NextPage = pageNum;
            }
            CurrentPage = pageNum;
            JumpPage = pageNum;
            PreviousPage = ((pageNum - 1) < 0) ? pageNum : pageNum - 1;
            EndIndex = StartIndex + NumPerPage - 1;
        }
    }
}
