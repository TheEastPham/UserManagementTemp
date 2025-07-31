import { Form, Input, Button, Avatar, Typography } from 'antd';
import { useState } from 'react';

export default function ProfilePage() {
  const [loading, setLoading] = useState(false);
  const onFinish = (values: any) => {
    setLoading(true);
    // TODO: handle update profile logic
    setTimeout(() => setLoading(false), 1000);
  };
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-blue-200">
      <div className="w-full max-w-lg bg-white rounded-xl shadow-lg p-8">
        <div className="flex flex-col items-center mb-6">
          <Avatar size={64} className="bg-blue-500 mb-2">U</Avatar>
          <Typography.Title level={3} className="text-center !text-blue-600">Thông tin cá nhân</Typography.Title>
        </div>
        <Form layout="vertical" onFinish={onFinish}>
          <Form.Item name="email" label="Email" rules={[{ required: true, type: 'email' }]}> <Input className="py-2 px-3 border rounded focus:outline-none focus:ring-2 focus:ring-blue-400" /> </Form.Item>
          <Form.Item name="name" label="Tên" rules={[{ required: true }]}> <Input className="py-2 px-3 border rounded focus:outline-none focus:ring-2 focus:ring-blue-400" /> </Form.Item>
          <Form.Item name="password" label="Mật khẩu mới"> <Input.Password className="py-2 px-3 border rounded focus:outline-none focus:ring-2 focus:ring-blue-400" /> </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={loading} block className="!bg-blue-500 !hover:bg-blue-600 !border-none !rounded-lg py-2">Cập nhật</Button>
          </Form.Item>
        </Form>
      </div>
    </div>
  );
}
