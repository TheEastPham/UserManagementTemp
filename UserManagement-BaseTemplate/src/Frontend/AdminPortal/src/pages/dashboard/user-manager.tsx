import { useEffect } from 'react';
import { useRouter } from 'next/router';
import { useAuthStore } from '@/lib/store/auth';
import UserTable from '../../components/UserTable';
import { Typography } from 'antd';

export default function UserManagerPage() {
  const router = useRouter();
  const authStore = useAuthStore();

  useEffect(() => {
    // Redirect to login if not authenticated
    if (!authStore.isAuthenticated || !authStore.user) {
      router.replace('/login');
      return;
    }
    // Redirect Member to profile
    const roles = authStore.user.roles || [];
    const primaryRole = authStore.user.role || roles[0];
    if (!roles.includes('Admin') && primaryRole !== 'Admin') {
      router.replace('/profile');
    }
  }, [authStore.isAuthenticated, authStore.user, router]);

  return (
    <div style={{ padding: 24 }}>
      <Typography.Title level={2}>Quản lý người dùng</Typography.Title>
      <UserTable />
    </div>
  );
}
