import axios from 'axios';

const api = axios.create({
  baseURL: '/api',
});

let mockUserId: string | null = null;

export function setMockUserId(userId: string) {
  mockUserId = userId;
}

api.interceptors.request.use((config) => {
  if (mockUserId) {
    config.headers['X-Mock-Line-User-Id'] = mockUserId;
  }
  return config;
});

export interface PlanDto {
  id: string;
  name: string;
  planType: string;
  priceYen: number;
  durationMinutes: number;
}

export interface PassDto {
  id: string;
  planName: string;
  status: string;
  validFrom: string;
  validTo: string;
  maxUses: number;
  usedCount: number;
  storeId: string;
  storeName: string;
  doorId: string;
  doorName: string;
  createdAt: string;
}

export interface QrTokenResponse {
  token: string;
  expiresAt: string;
  ttlSeconds: number;
}

export interface CheckoutResponse {
  checkoutUrl: string;
  orderId: string;
}

export const getPlans = (storeId?: string) =>
  api.get<PlanDto[]>('/plans', { params: { storeId } });

export const createCheckout = (planId: string, storeId: string, doorId: string) =>
  api.post<CheckoutResponse>('/checkout/create', {
    planId,
    storeId,
    doorId,
    successUrl: window.location.origin + '/checkout/return',
    cancelUrl: window.location.origin + '/',
  });

export const mockComplete = (orderId: string) =>
  api.post(`/checkout/mock-complete/${orderId}`);

export const getMyPasses = () =>
  api.get<PassDto[]>('/passes/me');

export const generateQr = (passId: string) =>
  api.post<QrTokenResponse>(`/passes/${passId}/qr`);

export default api;
