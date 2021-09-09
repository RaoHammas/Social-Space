using System.Collections.Generic;

namespace SocialSpace
{
    public class WorkSpace
    {
        public int Id { get; set; }
        public string WorkSpaceName { get; set; }
        public string IsActive { get; set; }

        public List<WorkSpacePage> WorkSpaceItems { get; set; }
    }
}
