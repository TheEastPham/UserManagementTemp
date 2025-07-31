import { Form, Input, Button, Select } from 'antd';

export default function UserForm({ onSubmit, initialValues }: any) {
  return (
    <Form layout="vertical" onFinish={onSubmit} initialValues={initialValues}>
      <Form.Item name="name" label="Tên" rules={[{ required: true }]}> <Input /> </Form.Item>
      <Form.Item name="email" label="Email" rules={[{ required: true, type: 'email' }]}> <Input /> </Form.Item>
      <Form.Item name="roles" label="Role">
        <Select mode="multiple">
          <Select.Option value="admin">Admin</Select.Option>
          <Select.Option value="user">User</Select.Option>
        </Select>
      </Form.Item>
      <Form.Item>
        <Button type="primary" htmlType="submit">Lưu</Button>
      </Form.Item>
    </Form>
  );
}
