using System;

namespace MOYV
{
    public struct PanelCloseEvent
    {
        public AbstractBasePanel Panel;

        public PanelCloseEvent(AbstractBasePanel panel)
        {
            Panel = panel;
        }
    }

    public struct PanelShowEvent
    {
        public AbstractBasePanel Panel;

        public PanelShowEvent(AbstractBasePanel panel)
        {
            Panel = panel;
        }
    }
}