using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Web.Mvc;
using Publisher.Validation;

namespace Publisher.Models
{
    /// <summary>
    /// ビルドモデル
    /// </summary>
    public class BuildModel
    {
        /// <summary>
        /// guid
        /// (F5対策)
        /// </summary>
        public string Guid
        {
            get;
            set;
        }
    }
}