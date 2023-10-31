import { nanoid } from 'nanoid';
import create from 'zustand';

export type Notification = {
    id: string,
    type: 'info' | 'warning' | 'success' | 'error',
    title: string,
    message?: string
};

export const useNotificationStore = create<NotificationStore>((set) => ({
    notifications: [],
    addNotification: (notification) => set((state) => ({
        notifications: [...state.notifications, { id: nanoid(), ...notification }]
    })),
    dismissNotification: (id) => set((state) => ({
        notifications: state.notifications.filter((notification: Notification) => notification.id !== id)
    }))
}));

type NotificationStore = {
    notifications: Notification[],
    addNotification: (notification: Omit<Notification, 'id'>) => void;
    dismissNotification: (id: string) => void;
};