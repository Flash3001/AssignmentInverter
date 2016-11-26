using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

namespace AssignmentInverter.Helpers
{
    public static class EditorHelper
    {
        public static TextSpan GetSelectionSpan()
        {
            var selection = GetEditor().Selection;

            var startBuffer = selection.Start;
            var start = startBuffer.Position.Position;
            var length = selection.End.Position.Position - start;
            return new TextSpan(start, length);
        }

        public static IWpfTextView GetEditor()
        {
            // http://stackoverflow.com/questions/35220083/how-to-move-caret-to-a-specific-position-after-performing-code-fix
            IVsTextManager textManager = (IVsTextManager)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsTextManager));
            IVsTextView textView = null;
            textManager.GetActiveView(1, null, out textView);
            IComponentModel componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            return componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(textView);
        }
    }
}
