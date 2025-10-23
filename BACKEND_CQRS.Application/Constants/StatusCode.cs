using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRONTEND_CQRS.Application.Constants
{
    public class StatusCode
    {
        public const int OK = 200;
        public const int Created = 201;
        public const int BadRequest = 400;
        public const int NotFound = 404;
        public const int InternalServerError = 500;
    }
}
