﻿using Codeer.Friendly.Windows.KeyMouse;
using OpenQA.Selenium;
using OpenQA.Selenium.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Selenium.CefSharp.Driver
{
    public class CefSharpWebElement : IWebElement, IWrapsDriver
    {   
        readonly CefSharpDriver _driver;
     
        internal int Id { get; }

        public CefSharpWebElement(CefSharpDriver driver, int index)
        {
            _driver = driver;
            Id = index;
        }

        public IWebDriver WrappedDriver => _driver;

        public string TagName => (_driver.ExecuteScript(JS.GetTagName(this.Id)) as string)?.ToLower();

        public string Text => _driver.ExecuteScript(JS.GetInnerHTML(this.Id)) as string;

        public bool Enabled => !(bool)_driver.ExecuteScript(JS.GetDisabled(this.Id));

        public bool Selected => (bool)_driver.ExecuteScript(JS.GetSelected(this.Id));

        public Point Location
        {
            get
            {
                var x = ToInt(_driver.ExecuteScript(JS.GetBoundingClientRectX(this.Id)));
                var y = ToInt(_driver.ExecuteScript(JS.GetBoundingClientRectY(this.Id)));
                return new Point(x, y);
            }
        }

        public Size Size
        {
            get
            {
                var w = ToInt(_driver.ExecuteScript(JS.GetBoundingClientRectWidth(this.Id)));
                var h = ToInt(_driver.ExecuteScript(JS.GetBoundingClientRectHeight(this.Id)));
                return new Size(w, h);
            }
        }

        static int ToInt(object src)
        {
            switch (src)
            {
                case int val: return val;
                case long val: return (int)val;
                case float val: return (int)val;
                case double val: return (int)val;
                default: throw new NotSupportedException();
            }
        }

        public bool Displayed => (bool)_driver.ExecuteScript(JS.GetDisplayed(this.Id));

        public void Clear()
            => _driver.ExecuteScript(JS.SetAttribute(this.Id, "value", string.Empty));

        public void Click()
        {
            _driver.ExecuteScriptInternal(JS.Focus(Id));
            var pos = Location;
            var size = Size;
            pos.Offset(size.Width / 2, size.Height / 2);
            _driver.Click(MouseButtonType.Left, pos);
        }

        public string GetAttribute(string attributeName)
            => _driver.ExecuteScript(JS.GetAttribute(this.Id, attributeName)) as string;

        public string GetCssValue(string propertyName)
            => _driver.ExecuteScript(JS.GetCssValue(this.Id, propertyName)) as string;

        public string GetProperty(string propertyName)
            => _driver.ExecuteScript(JS.GetProperty(this.Id, propertyName)) as string;

        public void SendKeys(string text)
        {
            _driver.ExecuteScriptInternal(JS.Focus(Id));

            //TODO adjust text spec.

            _driver.Activate();
            _driver.App.SendKeys(text);
        }

        public void Submit()
            => _driver.ExecuteScript(JS.Submit(this.Id));

        public IWebElement FindElement(By by)
        {
            var text = by.ToString();
            var script = "";
            if (text.StartsWith("By.Id:"))
            {
                script = $@"
const element = window.__seleniumCefSharpDriver.getElementByEntryId({this.Id});
return element.querySelector('[id=""{text.Substring("By.Id:".Length).Trim()}""]');";
            }
            if (text.StartsWith("By.Name:"))
            {
                script = $@"
const element = window.__seleniumCefSharpDriver.getElementByEntryId({this.Id});
return element.querySelector('[name=""{text.Substring("By.Name:".Length).Trim()}""]');";
            }
            if (text.StartsWith("By.ClassName[Contains]:"))
            {
                script = $@"
const element = window.__seleniumCefSharpDriver.getElementByEntryId({this.Id});
return element.getElementsByClassName('{text.Substring("By.ClassName[Contains]:".Length).Trim()}')[0];";
            }
            if (text.StartsWith("By.CssSelector:"))
            {
                script = $@"
const element = window.__seleniumCefSharpDriver.getElementByEntryId({this.Id});
return element.querySelector(""{text.Substring("By.CssSelector:".Length).Trim()}"");";
            }
            if (text.StartsWith("By.TagName:"))
            {
                script = $@"
const element = window.__seleniumCefSharpDriver.getElementByEntryId({this.Id});
return element.getElementsByTagName('{text.Substring("By.TagName:".Length).Trim()}')[0];";
            }
            if (text.StartsWith("By.XPath:"))
            {
                script = $@"
const element = window.__seleniumCefSharpDriver.getElementByEntryId({this.Id});
return window.__seleniumCefSharpDriver.getElementsByXPath('{text.Substring("By.XPath:".Length).Trim()}', element)[0];";
            }
            if (!(_driver.ExecuteScript(script) is CefSharpWebElement result))
            {
                throw new NoSuchElementException($"Element not found: {text}");
            }
            return result;
        }

        public ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            var text = by.ToString();
            var script = "";
            if (text.StartsWith("By.Id:"))
            {
                script = $@"
const element = window.__seleniumCefSharpDriver.getElementByEntryId({this.Id});
return element.querySelectorAll('[id=""{text.Substring("By.Id:".Length).Trim()}""]');";
            }
            if (text.StartsWith("By.Name:"))
            {
                script = $@"
const element = window.__seleniumCefSharpDriver.getElementByEntryId({this.Id});
return element.querySelectorAll('[name=""{text.Substring("By.Name:".Length).Trim()}""]');";
            }
            if (text.StartsWith("By.ClassName[Contains]:"))
            {
                script = $@"
const element = window.__seleniumCefSharpDriver.getElementByEntryId({this.Id});
return element.getElementsByClassName('{text.Substring("By.ClassName[Contains]:".Length).Trim()}');";
            }
            if (text.StartsWith("By.CssSelector:"))
            {
                script = $@"
const element = window.__seleniumCefSharpDriver.getElementByEntryId({this.Id});
return element.querySelectorAll(""{text.Substring("By.CssSelector:".Length).Trim()}"");";
            }
            if (text.StartsWith("By.TagName:"))
            {
                script = $@"
const element = window.__seleniumCefSharpDriver.getElementByEntryId({this.Id});
return element.getElementsByTagName('{text.Substring("By.TagName:".Length).Trim()}');";
            }
            if (text.StartsWith("By.XPath:"))
            {
                script = $@"
const element = window.__seleniumCefSharpDriver.getElementByEntryId({this.Id});
return window.__seleniumCefSharpDriver.getElementsByXPath('{text.Substring("By.XPath:".Length).Trim()}', element);";
            }
            if (!(_driver.ExecuteScript(script) is ReadOnlyCollection<IWebElement> result))
            {
                return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
            }
            return result;
        }
    }
}
