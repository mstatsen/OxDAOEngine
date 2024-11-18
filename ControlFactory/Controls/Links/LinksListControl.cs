using OxLibrary;
using OxDAOEngine.Data;
using System.Diagnostics;
using OxLibrary.Controls;
using OxDAOEngine.Data.Links;
using OxLibrary.Dialogs;

namespace OxDAOEngine.ControlFactory.Controls.Links
{
    public class LinksListControl<TField, TDAO> 
        : ListItemsControl<Links<TField>, Link<TField>, LinkEditor<TField, TDAO>, TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override void InitButtons()
        {
            base.InitButtons();
            OxIconButton goButton = CreateButton(OxIcons.Go);
            goButton.ToolTipText = "Follow the link";
            PrepareViewButton(
                goButton,
                (s, e) =>
                {
                    if (SelectedItem is null)
                        return;

                    try
                    {
                        Process.Start(
                            new ProcessStartInfo
                            {
                                FileName = SelectedItem.Url,
                                UseShellExecute = true
                            }
                        );
                    }
                    catch
                    {
                        OxMessage.ShowError("Unable to follw this link. Check the Url please.", this);
                    }

                },
                true);
        }

        protected override string ItemName() => "Link";

        protected override string GetText() => "Links";
    }
}