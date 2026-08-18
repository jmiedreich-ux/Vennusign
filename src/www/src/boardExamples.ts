// These board styles are not invented for this page - they recreate the real
// layouts Vennusign screens render from src/display/src/layouts (Classic
// Chalkboard, Neon Chalkboard, Digital Tap Board, Tap Strips, Daily Special
// Hero, Classic Diner, Photo Grid), so what a visitor sees here is what a
// Vennusign screen actually looks like, not a mockup of one.

export type BoardItemTag = "new" | "chef-pick" | "limited" | "sold-out" | "popular";

export type BoardItem = {
  name: string;
  price: string;
  detail?: string;
  tag?: BoardItemTag;
};

export type BoardStyleKey =
  | "classic-diner"
  | "photo-grid"
  | "tap-strips"
  | "classic-chalkboard"
  | "neon-chalkboard"
  | "digital-tap-board"
  | "daily-special-hero";

export type BoardPeriod = {
  id: string;
  label: string;
  time: string;
  headline: string;
  style: BoardStyleKey;
  glow?: string;
  happyHourEndsLabel?: string;
  items: BoardItem[];
};

export type VenueExample = {
  id: "restaurant" | "bar";
  label: string;
  venueName: string;
  periods: BoardPeriod[];
};

export const venueExamples: VenueExample[] = [
  {
    id: "restaurant",
    label: "Restaurant",
    venueName: "VENNU BISTRO",
    periods: [
      {
        id: "breakfast",
        label: "Breakfast",
        time: "7:00 AM",
        headline: "Morning favorites",
        style: "classic-diner",
        items: [
          { name: "House roast", price: "$3" },
          { name: "Avocado toast", price: "$11", detail: "Sourdough, chili flake, lime", tag: "chef-pick" },
          { name: "Berry bowl", price: "$9" },
          { name: "Steel-cut oats", price: "$7" },
          { name: "Breakfast burrito", price: "$12", tag: "new" }
        ]
      },
      {
        id: "lunch",
        label: "Lunch",
        time: "11:30 AM",
        headline: "Lunch, plated",
        style: "photo-grid",
        items: [
          { name: "Market sandwich", price: "$14" },
          { name: "Tomato soup", price: "$7" },
          { name: "Citrus salad", price: "$12", tag: "popular" },
          { name: "Grilled chicken wrap", price: "$13" },
          { name: "Soup + half sandwich", price: "$11", tag: "chef-pick" },
          { name: "Roasted veg bowl", price: "$12" }
        ]
      },
      {
        id: "happy-hour",
        label: "Happy Hour",
        time: "4:00 PM",
        headline: "Happy hour, on now",
        style: "tap-strips",
        happyHourEndsLabel: "Ends 6:00 PM",
        items: [
          { name: "House wine", price: "$6" },
          { name: "Loaded fries", price: "$8" },
          { name: "Flatbread", price: "$9" },
          { name: "Draft beer", price: "$5" },
          { name: "Wings (6pc)", price: "$8", tag: "limited" }
        ]
      },
      {
        id: "dinner",
        label: "Dinner",
        time: "5:30 PM",
        headline: "Dinner",
        style: "classic-chalkboard",
        glow: "#68bfff",
        items: [
          { name: "Roasted salmon", price: "$26" },
          { name: "Garden pasta", price: "$21" },
          { name: "Chocolate tart", price: "$9" },
          { name: "Filet mignon", price: "$34", tag: "chef-pick" },
          { name: "Seasonal risotto", price: "$23", tag: "sold-out" }
        ]
      },
      {
        id: "late-night",
        label: "Late Night",
        time: "10:00 PM",
        headline: "Late night",
        style: "neon-chalkboard",
        items: [
          { name: "Truffle fries", price: "$10" },
          { name: "Cheese plate", price: "$14" },
          { name: "Espresso martini", price: "$12" },
          { name: "Midnight burger", price: "$15", tag: "new" }
        ]
      }
    ]
  },
  {
    id: "bar",
    label: "Bar",
    venueName: "VENNU TAP HOUSE",
    periods: [
      {
        id: "draft-list",
        label: "Draft List",
        time: "All day",
        headline: "16 taps, updated live",
        style: "digital-tap-board",
        items: [
          { name: "Hazy IPA", price: "$7", detail: "6.5% ABV" },
          { name: "Pilsner", price: "$6", detail: "4.8% ABV" },
          { name: "Stout", price: "$7", detail: "5.9% ABV" },
          { name: "Seasonal sour", price: "$8", detail: "5.2% ABV", tag: "new" },
          { name: "Cider", price: "$6", detail: "5.0% ABV", tag: "sold-out" },
          { name: "Amber lager", price: "$6", detail: "5.4% ABV" }
        ]
      },
      {
        id: "bar-happy-hour",
        label: "Happy Hour",
        time: "4:00 – 6:00 PM",
        headline: "$2 off every draft",
        style: "daily-special-hero",
        happyHourEndsLabel: "Ends 6:00 PM",
        items: [
          { name: "All drafts", price: "$2 off" },
          { name: "Well drinks", price: "$5" },
          { name: "Loaded nachos", price: "$9" },
          { name: "Pretzel bites", price: "$7", tag: "limited" }
        ]
      },
      {
        id: "game-day",
        label: "Game Day",
        time: "Kickoff – close",
        headline: "Game day specials",
        style: "photo-grid",
        items: [
          { name: "Bucket of domestics (5)", price: "$18", tag: "popular" },
          { name: "Wing platter", price: "$16", tag: "chef-pick" },
          { name: "Loaded nachos", price: "$10" },
          { name: "Shot specials", price: "$4" }
        ]
      },
      {
        id: "cocktail-hour",
        label: "Cocktail Hour",
        time: "7:00 PM",
        headline: "Cocktail list",
        style: "classic-chalkboard",
        glow: "#e6a6ff",
        items: [
          { name: "Old fashioned", price: "$13" },
          { name: "Espresso martini", price: "$12" },
          { name: "House margarita", price: "$11", tag: "chef-pick" },
          { name: "Smoked negroni", price: "$14", tag: "new" }
        ]
      }
    ]
  }
];

export const boardTagLabel: Record<BoardItemTag, string> = {
  "new": "New",
  "chef-pick": "Chef's pick",
  "limited": "Limited",
  "sold-out": "86'd",
  "popular": "Popular ★"
};
