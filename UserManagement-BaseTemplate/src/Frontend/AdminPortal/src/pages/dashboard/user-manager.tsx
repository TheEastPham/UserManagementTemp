import UserTable from '../../components/UserTable';
import { Typography } from 'antd';

export default function UserManagerPage() {
  return (
    <div style={{ padding: 24 }}>
      <Typography.Title level={2}>Quản lý người dùng</Typography.Title>
      <UserTable />
    </div>
  );
}
