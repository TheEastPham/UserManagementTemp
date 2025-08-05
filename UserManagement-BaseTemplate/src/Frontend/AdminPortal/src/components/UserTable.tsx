import { Table, Switch, Tag, Select, Button } from 'antd';
import { useEffect, useState } from 'react';
import { apiService } from '@/lib/services/api';
import type { User } from '@/types/userManagement';

const roles = ['Admin', 'Member'];

export default function UserTable() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    async function fetchUsers() {
      setLoading(true);
      try {
        const res = await apiService.getAllUsers({ page: 1, pageSize: 20 });
        setUsers(res.users || []);
      } catch (err) {
        // TODO: handle error
      } finally {
        setLoading(false);
      }
    }
    fetchUsers();
  }, []);

  const columns = [
    { title: 'Tên', dataIndex: 'firstName', key: 'firstName', render: (_: any, record: User) => `${record.firstName ?? ''} ${record.lastName ?? ''}` },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    { title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', render: (active: boolean, record: User) => (
      <Switch checked={active} disabled />
    ) },
    { title: 'Role', dataIndex: 'roles', key: 'roles', render: (roles: string[], record: User) => (
      <Select
        mode="multiple"
        value={roles}
        style={{ minWidth: 120 }}
        disabled
      >
        {roles.map(role => <Select.Option key={role} value={role}>{role}</Select.Option>)}
      </Select>
    ) },
    // ...existing code...
  ];

  return <Table columns={columns} dataSource={users} loading={loading} rowKey="id" pagination={{ pageSize: 20 }} />;
}
