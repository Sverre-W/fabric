import { apiBaseUrl } from '@/shared/api/client';

export const elsaApiBaseUrl = new URL(joinUrl(apiBaseUrl, '/elsa/api'), window.location.origin).toString();

function joinUrl(baseUrl: string, path: string) {
  if (!baseUrl) {
    return path;
  }

  return `${baseUrl.replace(/\/$/, '')}/${path.replace(/^\//, '')}`;
}
