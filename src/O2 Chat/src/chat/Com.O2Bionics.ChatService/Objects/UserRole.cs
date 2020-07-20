namespace Com.O2Bionics.ChatService.Objects
{
    public struct UserRole
    {
        public UserRoleCode Role { get; }
        public uint DepartmentId { get; }

        public UserRole(UserRoleCode role, uint departmentId = 0) : this()
        {
            Role = role;
            DepartmentId = departmentId;
        }

        public override string ToString()
        {
            var name = Role.ToString("G");
            if (Role == UserRoleCode.Agent || Role == UserRoleCode.Supervisor)
                name += " (" + DepartmentId + ")";
            return name;
        }
    }
}