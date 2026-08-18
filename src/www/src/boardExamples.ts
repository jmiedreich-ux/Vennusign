// These board styles are not invented for this page - they recreate the real
// layouts Vennusign screens render from src/display/src/layouts (Classic
// Chalkboard, Neon Chalkboard, Digital Tap Board, Tap Strips, Daily Special
// Hero, Classic Diner, Photo Grid), so what a visitor sees here is what a
// Vennusign screen actually looks like, not a mockup of one.

import marketSandwich from "./assets/food/market-sandwich.webp";
import tomatoSoup from "./assets/food/tomato-soup.webp";
import citrusSalad from "./assets/food/citrus-salad.webp";
import grilledChickenWrap from "./assets/food/grilled-chicken-wrap.webp";
import soupHalfSandwich from "./assets/food/soup-half-sandwich.webp";
import roastedVegBowl from "./assets/food/roasted-veg-bowl.webp";
import bucketOfDomestics from "./assets/food/bucket-of-domestics.webp";
import wingPlatter from "./assets/food/wing-platter.webp";
import loadedNachos from "./assets/food/loaded-nachos.webp";
import shotSpecials from "./assets/food/shot-specials.webp";
import happyHourBar from "./assets/food/happy-hour-bar.webp";
import hiitClass from "./assets/fitness/hiit-class.webp";
import yogaFlow from "./assets/fitness/yoga-flow.webp";
import strengthCircuit from "./assets/fitness/strength-circuit.webp";
import recoveryStretch from "./assets/fitness/recovery-stretch.webp";
import gymHero from "./assets/fitness/gym-hero.webp";
import popcorn from "./assets/cinema/popcorn.webp";
import nachosCinema from "./assets/cinema/nachos-cinema.webp";
import fountainSoda from "./assets/cinema/fountain-soda.webp";
import candyMix from "./assets/cinema/candy-mix.webp";
import cinemaHero from "./assets/cinema/cinema-hero.webp";
import posterWildfire from "./assets/cinema/poster-wildfire.webp";
import posterHorizon from "./assets/cinema/poster-horizon.webp";
import posterPaperMoons from "./assets/cinema/poster-papermoons.webp";
import posterMidnight from "./assets/cinema/poster-midnight.webp";
import generalTso from "./assets/chinese/general-tso.webp";
import sweetSourChicken from "./assets/chinese/sweet-sour-chicken.webp";
import beefBroccoli from "./assets/chinese/beef-broccoli.webp";
import loMein from "./assets/chinese/lo-mein.webp";
import loungeBreakfast from "./assets/airport/lounge-breakfast.webp";
import loungeSandwich from "./assets/airport/lounge-sandwich.webp";
import loungeEspresso from "./assets/airport/lounge-espresso.webp";
import loungeCocktail from "./assets/airport/lounge-cocktail.webp";
import boardingHero from "./assets/airport/boarding-hero.webp";
import linenShirt from "./assets/retail/linen-shirt.webp";
import canvasSneakers from "./assets/retail/canvas-sneakers.webp";
import woolScarf from "./assets/retail/wool-scarf.webp";
import leatherTote from "./assets/retail/leather-tote.webp";
import flashSaleHero from "./assets/retail/flash-sale-hero.webp";

export type BoardItemTag = "new" | "chef-pick" | "limited" | "sold-out" | "popular";

export type BoardItem = {
  name: string;
  price: string;
  detail?: string;
  tag?: BoardItemTag;
  photo?: string;
  /* movie-poster-board: showtime chips under the poster */
  times?: string[];
  /* flight-board: scheduled time and row status */
  timeLabel?: string;
  status?: "on-time" | "boarding" | "delayed" | "landed";
};

// Classic Chalkboard groups items into named categories in the real product
// (src/display/src/layouts/ClassicChalkboardLayout.tsx) - a category can carry
// one flat price for everything in it (shown as a badge), or leave price unset
// so each item shows its own.
export type BoardCategory = {
  name: string;
  price?: string;
  items: BoardItem[];
};

export type BoardStyleKey =
  | "classic-diner"
  | "photo-grid"
  | "tap-strips"
  | "classic-chalkboard"
  | "neon-chalkboard"
  | "digital-tap-board"
  | "daily-special-hero"
  | "movie-poster-board"
  | "flight-board"
  | "promo-splash"
  | "photo-tile-board"
  | "letterboard-special";

export type BoardPeriod = {
  id: string;
  label: string;
  time: string;
  headline: string;
  style: BoardStyleKey;
  glow?: string;
  happyHourEndsLabel?: string;
  photo?: string;
  items: BoardItem[];
  categories?: BoardCategory[];
};

export type VenueExample = {
  id: "restaurant" | "bar" | "fitness" | "cinema" | "airport" | "retail" | "chinese";
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
          { name: "Market sandwich", price: "$14", photo: marketSandwich },
          { name: "Tomato soup", price: "$7", photo: tomatoSoup },
          { name: "Citrus salad", price: "$12", tag: "popular", photo: citrusSalad },
          { name: "Grilled chicken wrap", price: "$13", photo: grilledChickenWrap },
          { name: "Soup + half sandwich", price: "$11", tag: "chef-pick", photo: soupHalfSandwich },
          { name: "Roasted veg bowl", price: "$12", photo: roastedVegBowl }
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
        items: [],
        categories: [
          {
            name: "Entrées",
            items: [
              { name: "Roasted salmon", price: "$26" },
              { name: "Garden pasta", price: "$21" },
              { name: "Filet mignon", price: "$34", tag: "chef-pick" },
              { name: "Seasonal risotto", price: "$23", tag: "sold-out" }
            ]
          },
          {
            name: "Dessert",
            items: [
              { name: "Chocolate tart", price: "$9" }
            ]
          }
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
        photo: happyHourBar,
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
          { name: "Bucket of domestics (5)", price: "$18", tag: "popular", photo: bucketOfDomestics },
          { name: "Wing platter", price: "$16", tag: "chef-pick", photo: wingPlatter },
          { name: "Loaded nachos", price: "$10", photo: loadedNachos },
          { name: "Shot specials", price: "$4", photo: shotSpecials }
        ]
      },
      {
        id: "cocktail-hour",
        label: "Cocktail Hour",
        time: "7:00 PM",
        headline: "Cocktail list",
        style: "classic-chalkboard",
        glow: "#e6a6ff",
        items: [],
        categories: [
          {
            name: "Classics",
            items: [
              { name: "Old fashioned", price: "$13" },
              { name: "Espresso martini", price: "$12" }
            ]
          },
          {
            name: "House Originals",
            items: [
              { name: "House margarita", price: "$11", tag: "chef-pick" },
              { name: "Smoked negroni", price: "$14", tag: "new" }
            ]
          }
        ]
      }
    ]
  },
  {
    id: "fitness",
    label: "Fitness Studio",
    venueName: "VENNU FITNESS",
    periods: [
      {
        id: "sunrise",
        label: "Sunrise",
        time: "6:00 AM",
        headline: "Early classes",
        style: "classic-diner",
        items: [
          { name: "Sunrise Vinyasa", price: "$22", detail: "6:00 AM · Studio A · Maya R.", tag: "new" },
          { name: "Power Cycle", price: "$20", detail: "6:30 AM · Studio B · Theo K." },
          { name: "Mat Pilates", price: "$18", detail: "7:00 AM · Studio A · Ines D.", tag: "popular" },
          { name: "Row & Core", price: "$20", detail: "7:15 AM · The Rig · Sam P." }
        ]
      },
      {
        id: "midday-burn",
        label: "Midday",
        time: "12:00 PM",
        headline: "Lunch break burn",
        style: "photo-grid",
        items: [
          { name: "Express HIIT", price: "$15", detail: "12:00 PM · The Rig · Theo K.", photo: hiitClass },
          { name: "Lunchtime Flow", price: "$16", detail: "12:15 PM · Studio A · Maya R.", photo: yogaFlow },
          { name: "Strength Circuit", price: "$18", detail: "12:30 PM · Studio B · Sam P.", tag: "popular", photo: strengthCircuit },
          { name: "Recovery Stretch", price: "$12", detail: "1:00 PM · Studio A · Ines D.", photo: recoveryStretch }
        ]
      },
      {
        id: "peak-hours",
        label: "Peak Hours",
        time: "5:30 PM",
        headline: "Evening peak, book ahead",
        style: "tap-strips",
        items: [
          { name: "Power Cycle", price: "$20", detail: "5:30 PM · Studio B · Theo K." },
          { name: "Vinyasa Flow", price: "$22", detail: "5:45 PM · Studio A · Maya R.", tag: "popular" },
          { name: "Strength & Sculpt", price: "$20", detail: "6:00 PM · The Rig · Sam P." },
          { name: "Hot Yoga", price: "$24", detail: "6:15 PM · Hot Room · Ines D.", tag: "limited" },
          { name: "HIIT Circuit", price: "$18", detail: "6:30 PM · The Rig · Theo K." }
        ]
      },
      {
        id: "night-session",
        label: "Night Owl",
        time: "8:30 PM",
        headline: "Night owl session",
        style: "neon-chalkboard",
        items: [
          { name: "Candlelight Yoga", price: "$20", detail: "8:30 PM · Studio A · Maya R." },
          { name: "Neon Cycle Party", price: "$25", detail: "9:00 PM · Studio B · DJ Theo", tag: "new" },
          { name: "Boxing Bootcamp", price: "$22", detail: "9:15 PM · The Rig · Sam P.", tag: "popular" },
          { name: "Sound Bath", price: "$18", detail: "10:00 PM · Studio A · Ines D." }
        ]
      },
      {
        id: "new-member",
        label: "New Member",
        time: "Any time",
        headline: "Your first month, on us",
        style: "daily-special-hero",
        photo: gymHero,
        items: [
          { name: "First class free", price: "Free" },
          { name: "Unlimited month", price: "$89" },
          { name: "Class 10-pack", price: "$150" },
          { name: "Personal intro session", price: "Free" }
        ]
      },
      {
        id: "membership-board",
        label: "Membership",
        time: "All day",
        headline: "Membership & packages",
        style: "classic-chalkboard",
        glow: "#ffb86b",
        items: [],
        categories: [
          {
            name: "Unlimited Plans",
            items: [
              { name: "Monthly Unlimited", price: "$89" },
              { name: "Annual Unlimited", price: "$780" }
            ]
          },
          {
            name: "Class Packs",
            items: [
              { name: "5-Class Pack", price: "$95" },
              { name: "10-Class Pack", price: "$150", tag: "popular" },
              { name: "20-Class Pack", price: "$260" }
            ]
          }
        ]
      }
    ]
  },
  {
    id: "cinema",
    label: "Cinema",
    venueName: "VENNU CINEMA",
    periods: [
      {
        id: "matinee",
        label: "Showtimes",
        time: "Today",
        headline: "Now playing",
        style: "movie-poster-board",
        items: [
          { name: "Wildfire Season", price: "$9", detail: "2h 4m · Action/Drama · PG-13", photo: posterWildfire, times: ["12:30", "4:15", "7:45"], tag: "new" },
          { name: "The Last Horizon", price: "$9", detail: "2h 16m · Sci-Fi · PG-13", photo: posterHorizon, times: ["12:15", "3:30", "8:00"] },
          { name: "Paper Moons", price: "$7", detail: "1h 32m · Family · G", photo: posterPaperMoons, times: ["1:00", "3:15", "6:30"], tag: "popular" },
          { name: "Midnight Static", price: "$9", detail: "1h 47m · Horror · R", photo: posterMidnight, times: ["4:15", "9:15", "11:30"] }
        ]
      },
      {
        id: "concessions",
        label: "Concessions",
        time: "All day",
        headline: "Snack bar",
        style: "photo-grid",
        items: [
          { name: "Buttered Popcorn (Large)", price: "$8", photo: popcorn },
          { name: "Loaded Nachos", price: "$7", photo: nachosCinema },
          { name: "Fountain Soda", price: "$5", photo: fountainSoda },
          { name: "Movie Candy Mix", price: "$4", photo: candyMix }
        ]
      },
      {
        id: "now-showing",
        label: "Now Showing",
        time: "Tonight",
        headline: "Tonight's premiere",
        style: "daily-special-hero",
        photo: cinemaHero,
        items: [
          { name: "Wildfire Season", price: "7:45 PM" },
          { name: "The Last Horizon", price: "8:00 PM" },
          { name: "Paper Moons (Kids)", price: "6:30 PM" },
          { name: "Midnight Static", price: "9:15 PM" }
        ]
      },
      {
        id: "marquee",
        label: "Marquee",
        time: "Evening",
        headline: "Tonight's lineup",
        style: "neon-chalkboard",
        items: [
          { name: "Wildfire Season", price: "$9", tag: "new" },
          { name: "Midnight Static", price: "$9", tag: "popular" },
          { name: "The Last Horizon", price: "$9" },
          { name: "Paper Moons (Kids)", price: "$7" }
        ]
      },
      {
        id: "combo-deals",
        label: "Bargain Tuesday",
        time: "Every Tuesday",
        headline: "Bargain Tuesday",
        style: "promo-splash",
        items: [
          { name: "All tickets, all shows", price: "$6" },
          { name: "Popcorn + drink combo", price: "$11" },
          { name: "Family combo (4)", price: "$28", tag: "popular" },
          { name: "Candy box", price: "$4" }
        ]
      }
    ]
  },
  {
    id: "airport",
    label: "Airport",
    venueName: "VENNU FIELD TERMINAL",
    periods: [
      {
        id: "departures",
        label: "Departures",
        time: "Live",
        headline: "Departures",
        style: "flight-board",
        items: [
          { name: "Denver", price: "B4", detail: "VN 118", timeLabel: "6:45", status: "on-time" },
          { name: "Chicago", price: "A12", detail: "VN 204", timeLabel: "7:10", status: "boarding" },
          { name: "Miami", price: "C7", detail: "VN 077", timeLabel: "7:25", status: "delayed" },
          { name: "Seattle", price: "B9", detail: "VN 361", timeLabel: "7:50", status: "on-time" },
          { name: "Austin", price: "A3", detail: "VN 145", timeLabel: "8:05", status: "on-time" }
        ]
      },
      {
        id: "arrivals",
        label: "Arrivals",
        time: "Live",
        headline: "Arrivals",
        style: "flight-board",
        items: [
          { name: "Boston", price: "D2", detail: "VN 512", timeLabel: "7:15", status: "landed" },
          { name: "Phoenix", price: "A8", detail: "VN 093", timeLabel: "7:40", status: "on-time" },
          { name: "New York (JFK)", price: "C1", detail: "VN 428", timeLabel: "7:55", status: "on-time" },
          { name: "Dallas", price: "B6", detail: "VN 260", timeLabel: "8:20", status: "delayed" }
        ]
      },
      {
        id: "lounge-menu",
        label: "Lounge",
        time: "All day",
        headline: "Executive lounge menu",
        style: "photo-grid",
        items: [
          { name: "Chef's breakfast plate", price: "$18", photo: loungeBreakfast },
          { name: "Artisan sandwich board", price: "$16", photo: loungeSandwich },
          { name: "Espresso bar", price: "$5", photo: loungeEspresso, tag: "popular" },
          { name: "Signature cocktail", price: "$14", photo: loungeCocktail }
        ]
      },
      {
        id: "gate-alerts",
        label: "Gate Alerts",
        time: "Boarding now",
        headline: "Now boarding",
        style: "daily-special-hero",
        photo: boardingHero,
        items: [
          { name: "Denver", price: "Gate B4" },
          { name: "Chicago", price: "Gate A12" },
          { name: "Miami", price: "Gate C7" },
          { name: "Seattle", price: "Gate B9" }
        ]
      },
      {
        id: "terminal-info",
        label: "Terminal Info",
        time: "All day",
        headline: "Terminal map & services",
        style: "classic-chalkboard",
        glow: "#5ec8d8",
        items: [],
        categories: [
          {
            name: "Dining",
            items: [
              { name: "Junction Café", price: "Gate B" },
              { name: "Noodle Bar", price: "Gate C" }
            ]
          },
          {
            name: "Shops",
            items: [
              { name: "Duty Free", price: "Gate A" },
              { name: "News & Gifts", price: "Gate D" }
            ]
          },
          {
            name: "Services",
            items: [
              { name: "Charging stations", price: "All gates" },
              { name: "Family restroom", price: "Gate B" }
            ]
          }
        ]
      }
    ]
  },
  {
    id: "retail",
    label: "Retail",
    venueName: "VENNU MERCANTILE",
    periods: [
      {
        id: "todays-deals",
        label: "Today's Deals",
        time: "All day",
        headline: "Today only",
        style: "promo-splash",
        items: [
          { name: "Winter coats", price: "40% off" },
          { name: "Denim jackets", price: "30% off" },
          { name: "Running shoes", price: "$20 off", tag: "popular" },
          { name: "Accessories", price: "BOGO 50%" }
        ]
      },
      {
        id: "new-arrivals",
        label: "New Arrivals",
        time: "This week",
        headline: "Just landed",
        style: "photo-grid",
        items: [
          { name: "Linen shirt", price: "$48", photo: linenShirt },
          { name: "Canvas sneakers", price: "$65", photo: canvasSneakers, tag: "new" },
          { name: "Wool scarf", price: "$32", photo: woolScarf },
          { name: "Leather tote", price: "$89", photo: leatherTote, tag: "popular" }
        ]
      },
      {
        id: "flash-sale",
        label: "Flash Sale",
        time: "2 hours only",
        headline: "Flash sale, ends soon",
        style: "daily-special-hero",
        happyHourEndsLabel: "Ends in 2 hours",
        photo: flashSaleHero,
        items: [
          { name: "Everything in store", price: "25% off" },
          { name: "Clearance rack", price: "50% off" },
          { name: "New markdowns", price: "Extra 10%" },
          { name: "Members", price: "Extra 5%" }
        ]
      },
      {
        id: "loyalty-board",
        label: "Rewards",
        time: "All day",
        headline: "Rewards & perks",
        style: "classic-chalkboard",
        glow: "#ff9f7a",
        items: [],
        categories: [
          {
            name: "Membership Tiers",
            items: [
              { name: "Silver", price: "Free" },
              { name: "Gold", price: "$25/yr" },
              { name: "Platinum", price: "$75/yr" }
            ]
          },
          {
            name: "Perks",
            items: [
              { name: "Birthday reward", price: "$10 off" },
              { name: "Free shipping", price: "Gold+" }
            ]
          }
        ]
      },
      {
        id: "store-info",
        label: "Store Info",
        time: "All day",
        headline: "Store hours & help",
        style: "classic-diner",
        items: [
          { name: "Store hours", price: "9–9" },
          { name: "Returns", price: "30 days" },
          { name: "Curbside pickup", price: "Free" },
          { name: "Gift wrapping", price: "$3" }
        ]
      }
    ]
  },
  {
    id: "chinese",
    label: "Chinese Takeout",
    venueName: "VENNU GOLDEN WOK",
    periods: [
      {
        id: "lunch-special",
        label: "Lunch Special",
        time: "11 AM – 3 PM",
        headline: "Lunch Special",
        style: "letterboard-special",
        items: [
          { name: "Chicken Chow Mein", price: "7.25" },
          { name: "Roast Pork Chow Mein", price: "7.25" },
          { name: "Sweet & Sour Chicken", price: "7.95" },
          { name: "Pepper Steak w. Onion", price: "8.25" },
          { name: "Beef w. Broccoli", price: "8.25" },
          { name: "Shrimp w. Lobster Sauce", price: "8.50" },
          { name: "Moo Goo Gai Pan", price: "7.95" },
          { name: "General Tso's Chicken", price: "8.50", tag: "popular" },
          { name: "Shrimp Lo Mein", price: "7.95" },
          { name: "B-B-Q Spare Ribs", price: "8.95" },
          { name: "Hunan Chicken", price: "7.95" },
          { name: "Boneless Chicken", price: "7.95" }
        ]
      },
      {
        id: "house-favorites",
        label: "House Favorites",
        time: "All day",
        headline: "House favorites",
        style: "photo-tile-board",
        items: [
          { name: "General Tso's Chicken", price: "$10.95", photo: generalTso, tag: "popular" },
          { name: "Sweet & Sour Chicken", price: "$9.95", photo: sweetSourChicken },
          { name: "Beef w. Broccoli", price: "$11.25", photo: beefBroccoli },
          { name: "House Special Lo Mein", price: "$9.75", photo: loMein }
        ]
      },
      {
        id: "combo-platters",
        label: "Combos",
        time: "All day",
        headline: "Combination platters",
        style: "photo-tile-board",
        items: [
          { name: "C1 · General Tso's Combo", price: "$11.50", detail: "w. pork fried rice & egg roll", photo: generalTso },
          { name: "C2 · Sweet & Sour Combo", price: "$10.75", detail: "w. pork fried rice & egg roll", photo: sweetSourChicken },
          { name: "C3 · Beef Broccoli Combo", price: "$11.75", detail: "w. pork fried rice & egg roll", photo: beefBroccoli, tag: "chef-pick" }
        ]
      },
      {
        id: "family-dinner",
        label: "Family Dinner",
        time: "After 4 PM",
        headline: "Family dinner",
        style: "promo-splash",
        items: [
          { name: "Feeds four", price: "$39.95" },
          { name: "2 large entrées", price: "Your pick" },
          { name: "Pork fried rice", price: "Large" },
          { name: "4 egg rolls + soup", price: "Included" }
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
