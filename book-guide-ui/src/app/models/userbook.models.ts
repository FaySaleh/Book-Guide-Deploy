export interface UserBook {
  id: number;
  userId: number;
  externalBookId: string;
  title?: string | null;
  author?: string | null;
  coverUrl?: string | null;
  status: number;      
  rating?: number | null;
  createdAt: string;
}
