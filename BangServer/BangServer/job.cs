using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BangServer
{
    public abstract class job
    {
        public string jobName;
        public int addLive;

        public abstract void jobSettings();
    }

    public class Sceriffo : job
    {
        public override void jobSettings()
        {
            jobName = "SCERIFFO";
            addLive = 1;
        }
    }

    public class Vice : job
    {
        public override void jobSettings()
        {
            jobName = "VICE";
            addLive = 0;
        }
    }

    public class Fuorilegge : job
    {
        public override void jobSettings()
        {
            jobName = "FUORILEGGE";
            addLive = 0;
        }
    }

    public class Rinnegato : job
    {
        public override void jobSettings()
        {
            jobName = "RINNEGATO";
            addLive = 0;
        }
    }
}
