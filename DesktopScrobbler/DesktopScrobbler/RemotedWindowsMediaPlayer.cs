// Written by Jonathan Dibble, Microsoft Corporation
// CODE IS PROVIDED AS-IS WITH NO WARRIENTIES EXPRESSED OR IMPLIED.

namespace WindowsMediaPlayerScrobblePlugin 
{
	using System;
	using System.Windows.Forms;
	using System.Runtime.InteropServices;


	/// <summary>
	/// This is the actual Windows Media Control.
	/// </summary>
	[System.Windows.Forms.AxHost.ClsidAttribute("{6bf52a52-394a-11d3-b153-00c04f79faa6}")]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class RemotedWindowsMediaPlayer : System.Windows.Forms.AxHost,
		IOleServiceProvider,
		IOleClientSite
	{

        private IOleObject _oleObject = null;
        private WMPLib.IWMPPlayer4 _controlOcx;

        private bool _isControlLoaded = false;
        private bool _isControlSited = false;

        public void DoInterfaceAttachment()
        {
            if (!_isControlLoaded)
            {
                try
                {
                    //Get the IOleObject for Windows Media Player.
                    _oleObject = this.GetOcx() as IOleObject;
                    _isControlLoaded = true;
                }
                catch (System.Exception ex)
                {
                    //System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }

            if (!_isControlSited)
            {
                try
                {
                    //Set the Client Site for the WMP control.
                    _oleObject.SetClientSite(this as IOleClientSite);
                    _isControlSited = true;
                }
                catch (Exception ex)
                {
                    // This doesn't appear to have any effect on things, so once fired
                    // don't attempt to fire it again
                    _isControlSited = true;
                }
            }

            if (_controlOcx == null)
            {
                _controlOcx = ((WMPLib.IWMPPlayer4)(this.GetOcx()));
            }

        }

        #region IOleServiceProvider Members - Working
        /// <summary>
        /// During SetClientSite, WMP calls this function to get the pointer to <see cref="RemoteHostInfo"/>.
        /// </summary>
        /// <param name="guidService">See MSDN for more information - we do not use this parameter.</param>
        /// <param name="riid">The Guid of the desired service to be returned.  For this application it will always match
        /// the Guid of <see cref="IWMPRemoteMediaServices"/>.</param>
        /// <returns></returns>
        IntPtr IOleServiceProvider.QueryService(ref Guid guidService, ref Guid riid)
		{
			//If we get to here, it means Media Player is requesting our IWMPRemoteMediaServices interface
			if (riid == new Guid("cbb92747-741f-44fe-ab5b-f1a48f3b2a59"))
			{
				IWMPRemoteMediaServices iwmp = new RemoteHostInfo();
				return Marshal.GetComInterfaceForObject(iwmp, typeof(IWMPRemoteMediaServices));
			}

            return IntPtr.Zero;
			//throw new System.Runtime.InteropServices.COMException("No Interface", (int) HResults.E_NOINTERFACE);
		}
		#endregion

		#region IOleClientSite Members
		/// <summary>
		/// Not in use.  See MSDN for details.
		/// </summary>
		/// <exception cref="System.Runtime.InteropServices.COMException">E_NOTIMPL</exception>
		void IOleClientSite.SaveObject() 
		{
			//throw new System.Runtime.InteropServices.COMException("Not Implemented", (int) HResults.E_NOTIMPL);
		}

		/// <summary>
		/// Not in use.  See MSDN for details.
		/// </summary>
		/// <exception cref="System.Runtime.InteropServices.COMException"></exception>
		object IOleClientSite.GetMoniker(uint dwAssign, uint dwWhichMoniker)
		{
            return null;
			//throw new System.Runtime.InteropServices.COMException("Not Implemented", (int) HResults.E_NOTIMPL);
		}

		/// <summary>
		/// Not in use.  See MSDN for details.
		/// </summary>
		/// <exception cref="System.Runtime.InteropServices.COMException"></exception>
		object IOleClientSite.GetContainer() 
		{
            return null;
			//throw new System.Runtime.InteropServices.COMException("Not Implemented", (int) HResults.E_NOTIMPL);
		}

		/// <summary>
		/// Not in use.  See MSDN for details.
		/// </summary>
		/// <exception cref="System.Runtime.InteropServices.COMException"></exception>
		void IOleClientSite.ShowObject() 		
		{
			//throw new System.Runtime.InteropServices.COMException("Not Implemented", (int) HResults.E_NOTIMPL);
		}

		/// <summary>
		/// Not in use.  See MSDN for details.
		/// </summary>
		/// <exception cref="System.Runtime.InteropServices.COMException"></exception>
		void IOleClientSite.OnShowWindow(bool fShow) 		
		{
			//throw new System.Runtime.InteropServices.COMException("Not Implemented", (int) HResults.E_NOTIMPL);
		}

		/// <summary>
		/// Not in use.  See MSDN for details.
		/// </summary>
		/// <exception cref="System.Runtime.InteropServices.COMException"></exception>
		void IOleClientSite.RequestNewObjectLayout() 		
		{
			//throw new System.Runtime.InteropServices.COMException("Not Implemented", (int) HResults.E_NOTIMPL);
		}

		#endregion          

		/// <summary>
		/// Default Constructor.
		/// </summary>
		public RemotedWindowsMediaPlayer() : 
			base("6bf52a52-394a-11d3-b153-00c04f79faa6") 
		{
		}

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.Runtime.InteropServices.DispIdAttribute(4)]
        public virtual WMPLib.IWMPControls Ctlcontrols
        {
            get
            {
                WMPLib.IWMPControls controls = null;

                // Check if the Remoted OCX has been initialized, and sited
                if (_oleObject == null || !_isControlLoaded || !_isControlSited)
                {
                    DoInterfaceAttachment();
                }

                if (_oleObject != null)
                {
                    try
                    {
                        controls = ((WMPLib.IWMPPlayer4)_oleObject).controls;
                    }
                    catch (COMException ex)
                    {
                    }
                }
                return controls;
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.Runtime.InteropServices.DispIdAttribute(10)]
        public virtual WMPLib.WMPPlayState playState
        {
            get
            {
                WMPLib.WMPPlayState playState = WMPLib.WMPPlayState.wmppsUndefined;

                if (_controlOcx != null)
                {
                    try
                    {
                        playState = _controlOcx.playState;
                    }
                    catch (COMException ex)
                    {
                    }
                }

                return playState;
            }
        }
    }   
}
