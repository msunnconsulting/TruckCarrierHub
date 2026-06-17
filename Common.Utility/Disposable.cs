namespace Common.Utility
{
    using System;

    /// <summary>
    /// Any class that implements IDisposable, can inherit from this class and override just DisposeCore method from this class to dispose any unmanaged code.
    /// This class is a utility class which dispose and handle GC properly with de-structure.
    /// </summary>
    public class Disposable : IDisposable
    {
        /// <summary>
        /// boolean variable for check is disposable or not
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Finalizes an instance of the Disposable class.
        /// </summary>
        ~Disposable()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// this is Dispose method
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This is a virtual DisposeCore Method 
        /// </summary>
        protected virtual void DisposeCore()
        {
        }

        /// <summary>
        /// This is Dispose method
        /// </summary>
        /// <param name="disposing">indicates that dispose is true or false</param>
        private void Dispose(bool disposing)
        {
            if (!this.isDisposed && disposing)
            {
                this.DisposeCore();
            }

            this.isDisposed = true;
        }
    }
}
