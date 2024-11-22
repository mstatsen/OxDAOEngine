using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings;
using OxDAOEngine.View.Types;
using OxLibrary;

namespace OxDAOEngine.View
{
    public delegate RootListDAO<TField, TDAO>? GetActualItemList<TField, TDAO>()
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new();

    public sealed class ItemsView<TField, TDAO> : OxPane
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public EventHandler? LoadingStarted;
        public EventHandler? LoadingEnded;
        public readonly OxPanelLayouter Layouter = new()
        {
            Dock = OxDock.Top,
            Visible = false
        };

        public void Clear()
        {
            Layouter.Clear();

            if (placedCards is not null)
            {
                foreach (OxPane pane in placedCards.Cast<OxPane>())
                    pane.Dispose();

                placedCards.Clear();
            }
        }

        public event GetActualItemList<TField, TDAO>? GetActualItemList;

        public void Fill()
        {
            StartLoading();

            try
            {
                RootListDAO<TField, TDAO>? itemList = GetActualItemList?.Invoke();

                if (ItemList is not null 
                    && ItemList.Equals(itemList))
                    return;

                ItemList = itemList;
                paginator.ObjectCount = 
                    ItemList is not null 
                        ? ItemList.Count 
                        : 0;
            }
            finally
            {
                EndLoading();
            }
        }

        public ItemsView(ItemsViewsType viewType) : base() 
        {
            Text = TypeHelper.Name(viewType);
            ViewType = viewType;
            paginator.PageSize = SettingsPageSize;
        }

        protected override void PrepareInnerControls()
        {
            paginator.Parent = this;
            base.PrepareInnerControls();
            /*
            Layouter.Parent = ContentBox;
            ContentBox.AutoScroll = true;
            */

            Layouter.Parent = this;
            AutoScroll = true;
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();
            paginator.BaseColor = BaseColor;
            Layouter.BaseColor = BaseColor;
        }

        protected override void SetHandlers()
        {
            base.SetHandlers();
            paginator.PageChanged += PaginatorPageChangedHandler;
        }

        public void RenewCards()
        {
            Fill();
            paginator.CurrentPage = 1;
        }

        private void PaginatorPageChangedHandler(object sender, OxPaginatorEventArgs e)
        {
            StartLoading();

            try
            {
                CreateAndLayoutCards(e);
            }
            finally
            {
                EndLoading();
            }
        }
            

        private void CreateAndLayoutCards(OxPaginatorEventArgs e)
        {
            if (ItemList is null)
            {
                Clear();
                return;
            }

            if (!paginator.PageSize.Equals(placedCards?.Count))
            {
                Clear();
                placedCards = CreateViews();
                Layouter.LayoutPanels(placedCards.AsPaneList);
            }

            e.EndObjectIndex = Math.Min(e.EndObjectIndex, ItemList.Count);

            if (ItemList.Count > 0)
            {
                Layouter.RealPlacedCount = e.EndObjectIndex - e.StartObjectIndex;

                int itemIndex = e.StartObjectIndex;

                foreach (IItemView<TField, TDAO> card in placedCards)
                    if (itemIndex < e.EndObjectIndex)
                    {
                        card.Item = ItemList[itemIndex];
                        itemIndex++;
                    }
            }
            else
                Layouter.RealPlacedCount = 0;
        }

        private ItemViewList<TField, TDAO> CreateViews()
        {
            ItemViewList<TField, TDAO> cards = new();

            for (int itemIndex = 0; itemIndex < paginator.PageSize; itemIndex++)
            {
                IItemView<TField, TDAO>? itemView = Factory.CreateItemView(ViewType, ItemViewMode.WithEditLinks);

                if (itemView is not null)
                {
                    itemView.ItemsView = this;
                    itemView.Visible = false;
                    cards.Add(itemView);
                }
            }

            return cards;
        }

        private readonly ControlFactory<TField, TDAO> Factory = DataManager.ControlFactory<TField, TDAO>();

        private void StartLoading()
        {
            Layouter.Visible = false;
            //ContentBox.Update();
            LoadingStarted?.Invoke(this, EventArgs.Empty);
        }

        private void EndLoading()
        {
            Layouter.Visible = true;
            Layouter.Size = new(Layouter.Width, Layouter.Height + 1);
            LoadingEnded?.Invoke(this, EventArgs.Empty);
        }

        private ItemViewList<TField, TDAO>? placedCards;
        private readonly ItemsViewsType ViewType;

        private static DAOSettings<TField, TDAO> Settings => 
            SettingsManager.DAOSettings<TField, TDAO>();

        private int MaximumCardsCount => 
            ViewType is ItemsViewsType.Icons 
                ? 270 
                : 18;

        private int SettingsPageSize => 
            ViewType is ItemsViewsType.Icons 
                ? Settings.IconsPageSize 
                : Settings.CardsPageSize;

        public void ApplySettings()
        {
            if (Settings.Observer[ViewType is ItemsViewsType.Icons
                    ? DAOSetting.IconsPageSize
                    : DAOSetting.CardsPageSize
                ])
            {
                int newPageSize = Math.Min(MaximumCardsCount, SettingsPageSize);

                if (!paginator.PageSize.Equals(newPageSize))
                    paginator.PageSize = newPageSize;
            }
            else
                if ((ViewType is ItemsViewsType.Icons 
                    && (Settings.Observer[DAOSetting.IconMapping]
                        || Settings.Observer[DAOSetting.IconsSize]))
                    || (ViewType is ItemsViewsType.Cards
                        && (Settings.Observer[DAOSetting.CardsAllowDelete]
                            || Settings.Observer[DAOSetting.CardsAllowEdit]
                            || Settings.Observer[DAOSetting.CardsAllowExpand]))
                )
                    RenewCards();
        }

        private readonly OxPaginator paginator = new()
        {
            Dock = OxDock.Top,
            PageSize = 1
        };

        private RootListDAO<TField, TDAO>? ItemList;
    }
}