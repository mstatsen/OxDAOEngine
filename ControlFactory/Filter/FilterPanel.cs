﻿using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Forms;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Filter.Types;
using OxLibrary.Handlers;

namespace OxDAOEngine.ControlFactory.Filter
{
    public delegate string GetCategoryName();

    public class FilterPanel<TField, TDAO> : OxFrameWithHeader
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly ControlBuilder<TField, TDAO> Builder;
        private readonly OxPanel ConcatControlParent = new();
        private readonly IControlAccessor ConcatControl;
        private readonly List<FilterGroupPanel<TField, TDAO>> groupsPanels = new();
        private readonly OxPanel AddGroupButtonParent = new();
        private readonly OxButton AddGroupButton = new("Add group", OxIcons.Plus);
        private readonly OxIconButton ViewFilterDescription = new(OxIcons.ViewInfo, 16)
        { 
            ToolTipText = "View filter description"
        };

        public GetCategoryName? GetCategoryName { get; set; }


        private short GroupPanelsHeight()
        {
            short result = 0;

            foreach (FilterGroupPanel<TField, TDAO> panel in groupsPanels)
                result |= panel.Height;

            return result;
        }

        public Filter<TField, TDAO> Filter
        {
            get => GrabFilter();
            set => FillFilter(value);
        }

        private void FillFilter(Filter<TField, TDAO> value)
        {
            Clear();
            ConcatControl.Value = value.FilterConcat;

            if (value.Count is 0)
                value.AddGroup(ConcatControl.EnumValue<FilterConcat>());

            foreach(FilterGroup<TField, TDAO> group in value)
                AddGroup(group);
        }

        private Filter<TField, TDAO> GrabFilter()
        {
            Filter<TField, TDAO> result = new(ConcatControl.EnumValue<FilterConcat>());

            foreach (FilterGroupPanel<TField, TDAO> groupPanel in groupsPanels)
                result.Add(groupPanel.Group);

            return result;
        }

        private void AddGroup(FilterGroup<TField, TDAO> group)
        {
            SuspendLayout();

            try
            {
                FilterGroupPanel<TField, TDAO> groupPanel = new(this, group, Builder, groupsPanels.Count)
                {
                    Parent = this,
                    Dock = OxDock.Top
                };
                groupPanel.RuleAdded += GroupPanelRuleAddedHandler;
                groupPanel.RemoveGroup += GroupPanelRemoveGroupHandler;
                groupsPanels.Insert(0, groupPanel);
                PrepareColors();

                foreach (FilterGroupPanel<TField, TDAO> panel in groupsPanels)
                {
                    panel.Dock = OxDock.None;
                    panel.Dock = OxDock.Top;
                }

                ConcatControlParent.Dock = OxDock.None;
                ConcatControlParent.Dock = OxDock.Top;
            }
            finally
            {
                ResumeLayout();
                //RecalcSize();
            }
        }

        public int GroupsCount => groupsPanels.Count;

        private void GroupPanelRemoveGroupHandler(object? sender, EventArgs e)
        {
            if (groupsPanels.Count is 1)
            {
                OxMessage.ShowError("You can't delete last group", this);
                return;
            }

            if (sender is not FilterGroupPanel<TField, TDAO> groupPanel)
                return;

            groupPanel.Parent = null;
            groupsPanels.Remove(groupPanel);
            //RecalcSize();
            //TODO: dispose group panel
        }

        private void GroupPanelRuleAddedHandler(object? sender, EventArgs e)
        {
            //RecalcSize();
            OnSizeChanged(OxSizeChangedEventArgs.Empty);
        }

        public FilterPanel(ControlBuilder<TField, TDAO> builder) : base()
        { 
            Builder = builder;
            ConcatControl = CreateConcatControl();
            PrepareAddGroupButton();
            Text = "Filter";
            Margin.Size = 8;
            //RecalcSize();
        }

        /*
        protected override short GetCalcedHeight() =>
            Header.Height
            | Margin.Top
            | Margin.Bottom
            | Padding.Top
            | Padding.Bottom
            | ConcatControlParent.CalcedHeight
            | GroupPanelsHeight()
            | AddGroupButton.Height
            | AddGroupButtonParent.Padding.Top
            | AddGroupButtonParent.Padding.Bottom;
        */


        private void PrepareAddGroupButton()
        {
            AddGroupButton.Parent = AddGroupButtonParent;
            AddGroupButton.Click += AddGroupButtonClickHandler;
            AddGroupButton.Height = 20;
            AddGroupButtonParent.Parent = this;
            AddGroupButtonParent.Dock = OxDock.Bottom;
            AddGroupButtonParent.Padding.Size = 8;
            AddGroupButtonParent.Size = new(1, AddGroupButton.Height);
        }

        private void AddGroupButtonClickHandler(object? sender, EventArgs e) => 
            AddGroup(new FilterGroup<TField, TDAO>());

        public override void PrepareColors()
        {
            base.PrepareColors();

            foreach (FilterGroupPanel<TField, TDAO> groupPanel in groupsPanels)
                groupPanel.BaseColor = BaseColor;

            if (ConcatControlParent is not null)
                ConcatControlParent.BaseColor = BaseColor;

            if (ConcatControl is not null)
                ControlPainter.ColorizeControl(ConcatControl, BaseColor);

            if (AddGroupButtonParent is not null)
                AddGroupButtonParent.BaseColor = BaseColor;

            AddGroupButton.BaseColor = BaseColor;
        }

        private IControlAccessor CreateConcatControl()
        {
            IControlAccessor result = Builder.Accessor("Filter:Concat", FieldType.Enum);
            result.Parent = ConcatControlParent;
            result.Left = -1;
            result.Top = -1;
            result.Width = 108;
            result.Height = 18;
            ConcatControlParent.Parent = this;
            ConcatControlParent.Size = new(1, (short)result.Height);
            ConcatControlParent.Dock = OxDock.Top;
            return result;
        }

        public void Clear()
        {
            foreach (FilterGroupPanel<TField, TDAO> groupPanel in groupsPanels)
            {
                groupPanel.Parent = null;
                //TODO: groupPanel.Dispose();
            }

            groupsPanels.Clear();
        }

        private string CategoryName =>
            GetCategoryName is null 
                ? string.Empty 
                : GetCategoryName.Invoke();

        protected override void PrepareInnerComponents()
        {
            base.PrepareInnerComponents();
            ViewFilterDescription.Click += ViewFilterDescriptionButtonClickHandler;
            Header.AddButton(ViewFilterDescription);
        }

        private void ViewFilterDescriptionButtonClickHandler(object? sender, EventArgs e) => 
            TextViewer.Show("Filter of '" + CategoryName + "' category", Filter.Description, BaseColor);
    }
}