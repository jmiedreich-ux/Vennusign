export type BoardItemTag = "new" | "chef-pick" | "limited" | "sold-out";

export type BoardItem = {
  name: string;
  price: string;
  tag?: BoardItemTag;
};

export type BoardPeriod = {
  id: string;
  label: string;
  time: string;
  headline: string;
  style: string;
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
        headline: "Morning favorites are live",
        style: "bright",
        items: [
          { name: "House roast", price: "$3" },
          { name: "Avocado toast", price: "$11", tag: "chef-pick" },
          { name: "Berry bowl", price: "$9" },
          { name: "Steel-cut oats", price: "$7" },
          { name: "Breakfast burrito", price: "$12", tag: "new" }
        ]
      },
      {
        id: "lunch",
        label: "Lunch",
        time: "11:30 AM",
        headline: "Lunch menu switches on time",
        style: "teal",
        items: [
          { name: "Market sandwich", price: "$14" },
          { name: "Tomato soup", price: "$7" },
          { name: "Citrus salad", price: "$12" },
          { name: "Grilled chicken wrap", price: "$13" },
          { name: "Soup + half sandwich", price: "$11", tag: "chef-pick" }
        ]
      },
      {
        id: "happy-hour",
        label: "Happy Hour",
        time: "4:00 PM",
        headline: "Happy hour pricing is on",
        style: "amber",
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
        headline: "Dinner presentation is ready",
        style: "gold",
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
        headline: "Late-night menu is live",
        style: "midnight",
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
        style: "chalkboard",
        items: [
          { name: "Hazy IPA", price: "$7" },
          { name: "Pilsner", price: "$6" },
          { name: "Stout", price: "$7" },
          { name: "Seasonal sour", price: "$8", tag: "new" },
          { name: "Cider", price: "$6", tag: "sold-out" }
        ]
      },
      {
        id: "bar-happy-hour",
        label: "Happy Hour",
        time: "4:00 – 6:00 PM",
        headline: "$2 off every draft",
        style: "deal",
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
        headline: "Game day specials are on",
        style: "live",
        items: [
          { name: "Bucket of domestics (5)", price: "$18" },
          { name: "Wing platter", price: "$16", tag: "chef-pick" },
          { name: "Loaded nachos", price: "$10" },
          { name: "Shot specials", price: "$4" }
        ]
      },
      {
        id: "cocktail-hour",
        label: "Cocktail Hour",
        time: "7:00 PM",
        headline: "The evening cocktail list",
        style: "cocktail",
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
  "sold-out": "86'd"
};
