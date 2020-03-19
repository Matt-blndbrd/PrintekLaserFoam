
//Usage statistics and Update are for PrintekLaserFoam official version
//Unofficial/fork versions of PrintekLaserFoam should use their own url for stats and update (if they need the feature)

//to leave the file in the repo but ignore future changes to it:
//git update-index --skip-worktree PrintekLaserFoam\UrlManager.cs

//https://fallengamer.livejournal.com/93321.html
//https://stackoverflow.com/questions/13630849/git-difference-between-assume-unchanged-and-skip-worktree

namespace PrintekLaserFoam
{
	public static class UrlManager
	{
		public static string UpdateMain = null;		//@"https://api.github.com/repos/arkypita/PrintekLaserFoam/releases/latest";
		public static string UpdateMirror = null;	//@"http://printek.it/laserfoamhelp/latest.php";
		public static string Statistics = null;		//@"http://stats.printek.it/laserfoamhelp/handler.php";
	}
}
