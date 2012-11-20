﻿using System;
using System.Web;
using System.Web.WebPages;
using System.Xml.Linq;
using Composite.Data;
using System.Threading;

namespace Composite.AspNet.Razor
{
    /// <summary>
    /// Defines a composite C1 razor control
    /// </summary>
	public abstract class CompositeC1WebPage : WebPage, IDisposable
	{
		private bool _disposed;
		private DataConnection _data;

        /// <summary>
        /// Gets a <see cref="DataConnection"/> object.
        /// </summary>
		public DataConnection Data
		{
			get { return _data ?? (_data = new DataConnection()); }
		}

        /// <summary>
        /// Gets a <see cref="SitemapNavigator"/> object.
        /// </summary>
		public SitemapNavigator Sitemap
		{
			get { return Data.SitemapNavigator; }
		}


        /// <summary>
        /// Gets the home page node.
        /// </summary>
		public PageNode HomePageNode
		{
			get { return Sitemap.CurrentHomePageNode; }
		}


        /// <summary>
        /// Gets the current page node.
        /// </summary>
		public PageNode CurrentPageNode
		{
			get { return Sitemap.CurrentPageNode; }
		}


        /// <summary>
        /// Includes a named Page Template Feature. Page Template Feature are managed in '~/App_Data/PageTemplateFeatures' 
        /// or via the C1 Console's Layout perspective. They contain html and functional snippets.
        /// </summary>
        /// <param name="featureName">Name of the Page Template Feature to include. Names do not include an extension.</param>
        /// <returns></returns>
        public IHtmlString PageTemplateFeature(string featureName)
        {
            return Html.C1().GetPageTemplateFeature(featureName);
        }


        /// <summary>
        /// Renders the specified XNode.
        /// </summary>
        /// <param name="xNode">The <see cref="XNode">XNode</see>.</param>
        /// <returns></returns>
        public IHtmlString Markup(XNode xNode)
        {
            return Html.C1().Markup(xNode);
        }


        /// <summary>
        /// Gets to letter ISO Language Name representing the pages language - use this like &lt;html lang="@Lang" /&gt;
        /// </summary>
        public string Lang
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            }
        }


        /// <exclude />
		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

        /// <exclude />
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_data.Dispose();
				}

				_disposed = true;
			}
		}

        /// <exclude />
		~CompositeC1WebPage()
		{
			Dispose(false);
		}
	}
}
