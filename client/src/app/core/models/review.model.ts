export interface ProductReview {
  id: string;
  productId: string;
  customerName: string;
  rating: number;
  comment: string | null;
  createdAt: string;
}

export interface CreateReviewRequest {
  rating: number;
  comment: string | null;
}

export interface ReviewEligibility {
  hasPurchased: boolean;
  alreadyReviewed: boolean;
}
