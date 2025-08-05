import '../src/app/globals.css';
import 'antd/dist/reset.css';
import { ConfigProvider } from 'antd';
import viVN from 'antd/locale/vi_VN';
import { AppProps } from 'next/app';

export default function MyApp({ Component, pageProps }: AppProps) {
  return (
    <ConfigProvider locale={viVN}>
      <Component {...pageProps} />
    </ConfigProvider>
  );
}
