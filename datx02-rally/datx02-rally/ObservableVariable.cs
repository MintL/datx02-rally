using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace datx02_rally
{
    class ObservableVariable<E>
    {
        private E data;
        public event EventHandler<EventArgs> DataChanged;

        public E Data
        {
            get { return data; }
            set
            {
                data = value;
                if (DataChanged != null)
                    DataChanged(this, EventArgs.Empty);
            }
        }

        public ObservableVariable(E data)
        {
            this.data = data;
        }
    }
}
