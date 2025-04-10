using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Zero.Core.Web.Request
{
    /// <summary>
    /// id请求模型
    /// </summary>
    public class IdRequest
    {
        /// <summary>
        /// ID
        /// </summary>
        [Required]
        public long? Id { get; set; }
    }
}
