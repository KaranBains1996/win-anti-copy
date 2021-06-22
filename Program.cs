using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CyAntiCopy
{
	internal static class NativeMethods
	{
		//Reference https://docs.microsoft.com/en-us/windows/desktop/dataxchg/wm-clipboardupdate
		public const int WM_CLIPBOARDUPDATE = 0x031D;
		//Reference https://www.pinvoke.net/default.aspx/Constants.HWND
		public static IntPtr HWND_MESSAGE = new IntPtr(-3);
		//Reference https://www.pinvoke.net/default.aspx/user32/AddClipboardFormatListener.html
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool AddClipboardFormatListener(IntPtr hwnd);
		//Reference https://www.pinvoke.net/default.aspx/user32.setparent
		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
	}

	public sealed class ClipboardNotification
	{
		const string PREVENT_PATH = "D";

		private class NotificationForm : Form
		{
			public NotificationForm()
			{
				// Turn the child window into a message-only window (refer to Microsoft docs)
				NativeMethods.SetParent(Handle, NativeMethods.HWND_MESSAGE);
				// Place window in the system-maintained clipboard format listener list
				NativeMethods.AddClipboardFormatListener(Handle);
			}

			protected override void WndProc(ref Message m)
			{
				// Listen for operating system messages
				if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
				{
					Console.WriteLine("CLIPBOARD_CHANGE");
					HandleClipboardChange();
				}
				// Called for any unhandled messages
				base.WndProc(ref m);
			}
		}

        public static void HandleClipboardChange()
        {
            try
            {
                IDataObject iData = new DataObject();
                iData = Clipboard.GetDataObject();

                if (iData.GetDataPresent(DataFormats.Rtf))
                    Console.WriteLine((string)iData.GetData(DataFormats.Rtf));
                else if (iData.GetDataPresent(DataFormats.Text))
                    Console.WriteLine((string)iData.GetData(DataFormats.Text));
                else if (iData.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] paths = (string[])iData.GetData(DataFormats.FileDrop);
                    Console.WriteLine(String.Join(", ", paths));
                    foreach (string path in paths)
                    {
                        if (path.ToUpper().StartsWith(PREVENT_PATH))
                        {
                            Clipboard.Clear();
                        }
                    }
                }
                else
                    Console.WriteLine("[Clipboard data is not RTF or ASCII Text or FileDrop]");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

		[STAThread]
		private static void Main(string[] args)
		{
			Application.Run(new NotificationForm());
		}
	}
}
