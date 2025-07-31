import { Table, Switch, Tag, Select, Button } from 'antd';
import { useState } from 'react';

const roles = ['admin', 'user'];
const initialUsers = [
  { key: 1, name: 'Nguyen Van A', email: 'a@email.com', active: true, roles: ['admin'] },
  { key: 2, name: 'Tran Thi B', email: 'b@email.com', active: false, roles: ['user'] },
];

export default function UserTable() {
  const [users, setUsers] = useState(initialUsers);
  const columns = [
    { title: 'Tên', dataIndex: 'name', key: 'name' },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    { title: 'Trạng thái', dataIndex: 'active', key: 'active', render: (active: boolean, record: any) => (
      <Switch checked={active} onChange={() => toggleActive(record.key)} />
    ) },
    { title: 'Role', dataIndex: 'roles', key: 'roles', render: (roles: string[], record: any) => (
      <Select
        mode="multiple"
        value={roles}
        style={{ minWidth: 120 }}
        onChange={vals => changeRoles(record.key, vals)}
      >
        {roles.map(role => <Select.Option key={role} value={role}>{role}</Select.Option>)}
      </Select>
    ) },
    { title: 'Hành động', key: 'action', render: (_: any, record: any) => (
      <Button danger onClick={() => deleteUser(record.key)}>Xóa</Button>
    ) },
  ];

  function toggleActive(key: number) {
    setUsers(users => users.map(u => u.key === key ? { ...u, active: !u.active } : u));
  }
  function changeRoles(key: number, vals: string[]) {
    setUsers(users => users.map(u => u.key === key ? { ...u, roles: vals } : u));
  }
  function deleteUser(key: number) {
    setUsers(users => users.filter(u => u.key !== key));
  }

  return <Table columns={columns} dataSource={users} pagination={{ pageSize: 5 }} />;
}
