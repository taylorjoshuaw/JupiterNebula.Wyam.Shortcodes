/*
   Copyright 2019 Joshua William Taylor <taylor.joshua88@gmail.com>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using shortid;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Shortcodes;

namespace JupiterNebula.Wyam.Shortcodes.TabBlock
{
    /// <summary>
    /// Shortcode to be used with Wyam's Markdown module which renders tabs and tab panes in (X)HTML
    /// with Bootstrap classes and attributes.
    /// </summary>
    public class TabBlock : IShortcode
    {
        #region Id Attribute Generation
        /// <summary>
        /// Create a unique id attribute value for a tab block instance
        /// </summary>
        /// <returns>Unique id attribute value for a tab block instance</returns>
        private static string CreateBaseId() => $"{nameof(TabBlock)}__{ShortId.Generate(true, false)}";

        /// <summary>
        /// Creates an id attribute value prefix for the elements making up the tabs and tab panes
        /// of a tab block
        /// </summary>
        /// <param name="baseId">The id attribute value from the owning tab block</param>
        /// <param name="tabNumber">The index of the tab</param>
        /// <returns>
        /// Id attribute value prefix for the elements making up the tabs and tab panes of a tab block
        /// </returns>
        private static string CreateTabId(string baseId, int tabNumber) => $"{baseId}-{tabNumber}";

        /// <summary>
        /// Creates an id attribute value for a tab's link element
        /// </summary>
        /// <param name="tabId">
        /// Id attribute value prefix generated by <see cref="CreateTabId(string, int)"/> for the tab
        /// </param>
        /// <returns>Id attribute value for a tab's link element</returns>
        private static string CreateTabLinkId(string tabId) => tabId + "-link";

        /// <summary>
        /// Creates an id attribute value for a tab pane
        /// </summary>
        /// <param name="tabId">
        /// Id attribute value prefix generated by <see cref="CreateTabId(string, int)"/> for the tab
        /// </param>
        /// <returns>Id attribute value for a tab pane</returns>
        private static string CreateTabPaneId(string tabId) => tabId + "-pane";
        #endregion

        #region Tab List
        /// <summary>
        /// Creates a link for a Bootstrap-compatible tab as an "a" element to a tab pane anchor
        /// </summary>
        /// <param name="labelNode">Node to be placed into the generated "a" element to serve as a label</param>
        /// <param name="active">Whether this tab is to be marked as selected by the tab block</param>
        /// <param name="tabId">
        /// Id attribute for this tab as generated by <see cref="CreateTabId(string, int)"/>
        /// </param>
        /// <returns></returns>
        public static XElement CreateTabLink(XNode labelNode, bool active, string tabId)
        {
            var linkElement = new XElement("a", labelNode);
            var tabPaneId = CreateTabPaneId(tabId);

            linkElement.SetAttributeValue("class", $"nav-link{(active ? " active" : string.Empty)}");
            linkElement.SetAttributeValue("data-toggle", "tab");
            linkElement.SetAttributeValue("aria-selected", XmlConvert.ToString(active));
            linkElement.SetAttributeValue("aria-controls", tabPaneId);
            linkElement.SetAttributeValue("href", '#' + tabPaneId);
            linkElement.SetAttributeValue("id", CreateTabLinkId(tabId));

            return linkElement;
        }

        /// <summary>
        /// Creates a Boostrap-compatible tab as an li element containing a link to a tab pane
        /// </summary>
        /// <param name="labelNode">Node to be used as the label for the created tab</param>
        /// <param name="active">Whether this tab is to be marked as selected by the tab block</param>
        /// <param name="tabId">
        /// Id attribute for this tab as generated by <see cref="CreateTabId(string, int)"/>
        /// </param>
        /// <returns>Boostrap-compatible tab as an li element containing a link to a tab pane</returns>
        public static XElement CreateTab(XNode labelNode, bool active, string tabId)
        {
            var tabElement = new XElement("li", CreateTabLink(labelNode, active, tabId));
            tabElement.SetAttributeValue("class", "nav-item");

            return tabElement;
        }

        /// <summary>
        /// Creates a Bootstrap-compatible tab list as a ul element containing tabs for each 
        /// tab provided by <paramref name="shortcodeTabs"/>
        /// </summary>
        /// <param name="shortcodeTabs">XElements containing tab labels for each tab in the tab block</param>
        /// <param name="labelSelector">
        /// Transform function to select the label from a <paramref name="shortcodeTabs"/> element
        /// </param>
        /// <param name="baseId">The id attribute value from the owning tab block</param>
        /// <returns></returns>
        public static XElement CreateTabList(IEnumerable<XElement> shortcodeTabs, 
            Func<XElement, XNode> labelSelector, string baseId)
        {
            int tabCount = 0;
            var tabList =  new XElement("ul", 
                shortcodeTabs.Select(n => labelSelector(n))
                             .Select(n => CreateTab(n, tabCount == 0, CreateTabId(baseId, tabCount++))));
            
            tabList.SetAttributeValue("class", "nav nav-tabs");
            tabList.SetAttributeValue("role", "tablist");

            return tabList;
        }
        #endregion

        #region Tab Panes
        /// <summary>
        /// Creates a Bootstrap-compatible tab pane as a div element containing <paramref name="contentNodes"/>
        /// </summary>
        /// <param name="contentNodes">All nodes to be placed inside of the generate tab pane div element</param>
        /// <param name="active">Whether this pane corresponds to a selected tab in the tab block</param>
        /// <param name="tabId">
        /// Id attribute for this tab pane as generated by <see cref="CreateTabId(string, int)"/>
        /// </param>
        /// <returns>
        /// Bootstrap-compatible tab pane as a div element containing <paramref name="contentNodes"/>
        /// </returns>
        public static XElement CreateTabPane(IEnumerable<XNode> contentNodes, bool active, string tabId)
        {
            var tabPane = new XElement("div", contentNodes);

            tabPane.SetAttributeValue("class", $"tab-pane{(active ? " show active" : string.Empty)}");
            tabPane.SetAttributeValue("role", "tabpanel");
            tabPane.SetAttributeValue("aria-labelledby", CreateTabLinkId(tabId));
            tabPane.SetAttributeValue("id", CreateTabPaneId(tabId));

            return tabPane;
        }

        /// <summary>
        /// Creates a Bootstrap-compatible container div element containing tab panes for each provided tab
        /// </summary>
        /// <param name="shortcodeTabs">XElements to be made into tab panes inside the container</param>
        /// <param name="contentSelector">
        /// Transform function to select the child elements for tab panes from a <paramref name="shortcodeTabs"/> element
        /// </param>
        /// <param name="baseId">The id attribute value from the owning tab block</param>
        /// <returns>Bootstrap-compatible container div element containing tab panes for each provided tab</returns>
        public static XElement CreateTabPaneContainer(IEnumerable<XElement> shortcodeTabs, 
            Func<XElement, IEnumerable<XNode>> contentSelector, string baseId)
        {
            int tabCount = 0;

            var tabContent = new XElement("div", 
                shortcodeTabs.Select(contentSelector)
                             .Select(n => CreateTabPane(n, tabCount == 0, CreateTabId(baseId, tabCount++))));

            tabContent.SetAttributeValue("class", "tab-content");

            return tabContent;
        }
        #endregion

        /// <summary>
        /// Creates Bootstrap-compatible tabs and tab panes wrapped in a div element
        /// </summary>
        /// <param name="shortcodeTabs">
        /// XElements to be made into tabs and tab panes using <paramref name="tabLabelSelector"/>
        /// and <paramref name="tabContentSelector"/> for the tabs' labels and panes (respectively)
        /// </param>
        /// <param name="tabLabelSelector">
        /// Transform function to select a label XNode from a <paramref name="shortcodeTabs"/> element
        /// </param>
        /// <param name="tabContentSelector">
        /// Transform function to select the child elements from a <paramref name="shortcodeTabs"/> element
        /// </param>
        /// <returns>Bootstrap-compatible tabs and tab panes wrapped in a div element</returns>
        public static XElement CreateTabBlock(IEnumerable<XElement> shortcodeTabs, 
            Func<XElement, XNode> tabLabelSelector, Func<XElement, IEnumerable<XNode>> tabContentSelector)
        {
            var baseId = CreateBaseId();

            var tabBlock = new XElement("div", 
                CreateTabList(shortcodeTabs, tabLabelSelector, baseId),
                CreateTabPaneContainer(shortcodeTabs, tabContentSelector, baseId));

            tabBlock.SetAttributeValue("class", "tab-block");
            tabBlock.SetAttributeValue("id", baseId);

            return tabBlock;
        }

        /// <summary>
        /// Produces an IShortcodeResult of (X)HTML markup for Bootstrap-compatible tabs and tab panes wrapped in a div.
        /// </summary>
        /// 
        /// <param name="args">
        /// Key value pairs for shortcode arguments.
        ///   <remarks>All valid arguments are described in <see href="./README.md">the readme</see></remarks>
        /// </param>
        /// <param name="content">
        /// Content of the shortcode after being rendered into HTML by the Markdown module.
        ///   <remarks>
        ///   The provided HTML must be able to be parsed by XmlReader. Each child element under the root element will
        ///   become a tab. The first child node of each of these children will be used as a tab label, and the remaining
        ///   nodes will be placed inside of their corresponding tab pane.
        ///   </remarks>
        /// </param>
        /// <param name="document">Reference to the Markdown documenting containing this shortcode instance</param>
        /// <param name="context">Wyam execution context</param>
        /// 
        /// <returns>
        /// IShortcode result of (X)HTML markup for Bootstrap-compatible tabs and tab panes wrapped in a div
        /// </returns>
        /// 
        /// <exception cref="ArgumentException"><paramref name="content"/> is not valid XHTML</exception>
        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content,
            IDocument document, IExecutionContext context)
        {
            XElement contentElement;

            try
            {
                contentElement = XElement.Parse(content);
            }
            catch (XmlException e)
            {
                throw new ArgumentException(
                    $"{nameof(TabBlock)} shortcode requires Markdown content that can be rendered into a valid XHTML element.",
                    nameof(content), e);
            }

            var tabBlock = CreateTabBlock(contentElement.Elements(), n => n.FirstNode, n => n.Nodes().Skip(1));
            return context.GetShortcodeResult(tabBlock.ToString(SaveOptions.DisableFormatting));
        }
    }
}