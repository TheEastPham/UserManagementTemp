import { Form, Input, Button, Typography } from 'antd';
import { useState } from 'react';

export default function LoginPage() {
  const [loading, setLoading] = useState(false);
  const onFinish = (values: any) => {
    setLoading(true);
    // TODO: handle login logic
    setTimeout(() => setLoading(false), 1000);
  };
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-blue-200">
      <div className="w-full max-w-md bg-white rounded-xl shadow-lg p-8">
        <Typography.Title level={2} className="text-center mb-6 !text-blue-600">Đăng nhập</Typography.Title>
        <Form layout="vertical" onFinish={onFinish}>
          <Form.Item name="email" label="Email" rules={[{ required: true, type: 'email' }]}> <Input className="py-2 px-3 border rounded focus:outline-none focus:ring-2 focus:ring-blue-400" /> </Form.Item>
          <Form.Item name="password" label="Mật khẩu" rules={[{ required: true }]}> <Input.Password className="py-2 px-3 border rounded focus:outline-none focus:ring-2 focus:ring-blue-400" /> </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={loading} block className="!bg-blue-500 !hover:bg-blue-600 !border-none !rounded-lg py-2">Đăng nhập</Button>
          </Form.Item>
        </Form>
      </div>
    </div>
  );
}
