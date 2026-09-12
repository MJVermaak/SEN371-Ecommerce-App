import { useState } from 'react';
import { fallbackImage } from '../lib/shopping';

export default function ShopImage({ src, category, alt, ...props }) {
  const [failedSource, setFailedSource] = useState(null);
  const source = src && src !== failedSource ? src : fallbackImage(category);
  return <img {...props} src={source} alt={alt} onError={() => setFailedSource(src)} />;
}
