import { Form, Input, Button, Avatar } from 'antd';

export default function ProfileForm({ onSubmit, initialValues }: any) {
  return (
    <Form layout="vertical" onFinish={onSubmit} initialValues={initialValues}>
      <Form.Item name="email" label="Email" rules={[{ required: true, type: 'email' }]}> <Input /> </Form.Item>
      <Form.Item name="name" label="Tên" rules={[{ required: true }]}> <Input /> </Form.Item>
      <Form.Item name="avatar" label="Avatar">
        <Input />
      </Form.Item>
      <Form.Item name="password" label="Mật khẩu mới"> <Input.Password /> </Form.Item>
      <Form.Item>
        <Button type="primary" htmlType="submit">Cập nhật</Button>
      </Form.Item>
    </Form>
  );
}
